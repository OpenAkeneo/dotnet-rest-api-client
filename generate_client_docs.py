#!/usr/bin/env python3
"""
Generates OpenAkeneo.RestApiClient.ApiReference.md and OpenAkeneo.RestApiClient.ApiIndex.md
from the source code, oriented around .NET client methods (not raw HTTP endpoints).
"""

import re, os, json

# ── helpers ──────────────────────────────────────────────────────────────────

def read(path):
    with open(path, encoding="utf-8-sig") as f:
        return f.read()

# ── Extract methods from source ─────────────────────────────────────────────

SRC = os.path.join(os.path.dirname(__file__), "OpenAkeneo.RestApiClient")

METHOD_RE = re.compile(
    r'///\s*<summary>(.*?)</summary>(.*?)'
    r'public\s+async\s+([\w<>\[\],\s\?]+?)\s+(\w+Async)\((.*?)\)',
    re.DOTALL
)

PARAM_RE = re.compile(r'///\s*<param name="(\w+)">(.*?)</param>', re.DOTALL)
RETURNS_RE = re.compile(r'///\s*<returns>(.*?)</returns>', re.DOTALL)

def clean(s):
    """Strip XML doc tags and normalize whitespace."""
    # Replace <see cref="Type"/> with just the type name
    s = re.sub(r'<see\s+cref="([^"]*)"/>',  lambda m: m.group(1).split('.')[-1], s)
    s = re.sub(r'<c>(.*?)</c>', r'`\1`', s)
    s = re.sub(r'<paramref\s+name="([^"]*)"/>', r'`\1`', s)
    s = re.sub(r'<[^>]+>', '', s)
    s = re.sub(r'\s+', ' ', s).strip()
    return s

def parse_file(filepath):
    content = read(filepath)
    methods = []
    for m in METHOD_RE.finditer(content):
        summary = clean(m.group(1))
        doc_block = m.group(2)
        ret_type = m.group(3).strip()
        name = m.group(4)
        raw_params = m.group(5)

        # Parse params from doc comments
        params = []
        for pm in PARAM_RE.finditer(doc_block):
            pname = pm.group(1)
            pdesc = clean(pm.group(2))
            if pname == "ct":
                continue
            params.append((pname, pdesc))

        # Parse return type from doc
        ret_desc = ""
        rm = RETURNS_RE.search(doc_block)
        if rm:
            ret_desc = clean(rm.group(1))

        # Parse actual C# params for types using a regex that handles generics
        param_types = {}
        # Match: optional_keywords type<generic> name = default
        CPARAM = re.compile(r'(?:(?:\[.*?\]\s*)*)(?:this\s+)?(?:params\s+)?((?:[\w\?]+(?:<[^>]+>)?(?:\[\])?)(?:\?)?\s+)(\w+)(?:\s*=.*)?$')
        for chunk in re.split(r',\s*(?![^<>]*>)', raw_params):
            chunk = chunk.strip()
            cm = CPARAM.search(chunk)
            if cm:
                ptype = cm.group(1).strip()
                pname = cm.group(2).strip()
                param_types[pname] = ptype

        # Determine method category based on overload (skip dict-param overloads)
        if "Dictionary<string, string> queryParameters" in raw_params and name not in (
            "GetProductUuidListAsync", "GetProductIdentifierListAsync"
        ):
            # Skip the dictionary-overload variants (internal plumbing)
            # unless it's one that's genuinely useful
            if any(p[0] == "queryParameters" for p in params) and len(params) == 1:
                continue

        methods.append({
            "name": name,
            "summary": summary,
            "return_type": ret_type,
            "return_desc": ret_desc,
            "params": params,
            "param_types": param_types,
        })
    return methods


# ── Organize into resource groups ────────────────────────────────────────────

FILES_AND_GROUPS = [
    ("AkeneoContext.Products.cs", [
        ("Product UUID", "ProductUuid", "Products identified by UUID (recommended)."),
        ("Product Identifier", "ProductIdentifier", "Products identified by SKU (legacy)."),
        ("Product Model", "ProductModel", "Parent models for variant products."),
        ("Product Media File", "ProductMediaFile", "Binary media files attached to products."),
    ]),
    ("AkeneoContext.Attributes.cs", [
        ("Attribute", "AkeneoAttribute", "Product attributes (text, number, select, etc.)."),
        ("Attribute Option", "AttributeOption", "Options for simple/multi-select attributes."),
        ("Attribute Group", "AttributeGroup", "Logical groupings of attributes."),
    ]),
    ("AkeneoContext.Families.cs", [
        ("Family", "Family", "Product families defining required attributes."),
        ("Family Variant", "FamilyVariant", "Variant axis definitions within families."),
    ]),
    ("AkeneoContext.Categories.cs", [
        ("Category", "Category", "Product classification tree nodes."),
    ]),
    ("AkeneoContext.Channels.cs", [
        ("Channel", "Channel", "Sales channels (scopes) with locales and currencies."),
        ("Locale", "Locale", "Available locales (e.g. en_US, fr_FR)."),
        ("Currency", "Currency", "Available currencies (ISO 4217)."),
        ("Measurement Family", "MeasurementFamily", "Units of measure (weight, length, etc.)."),
    ]),
    ("AkeneoContext.AssociationTypes.cs", [
        ("Association Type", "AssociationType", "Product association types (cross-sell, up-sell, etc.)."),
    ]),
    ("AkeneoContext.ReferenceEntities.cs", [
        ("Reference Entity", "ReferenceEntity", "Structured reference data entities."),
        ("Reference Entity Attribute", "ReferenceEntityAttribute", "Attributes on reference entities."),
        ("Reference Entity Attribute Option", "ReferenceEntityAttributeOption", "Options for reference entity select attributes."),
        ("Reference Entity Record", "ReferenceEntityRecord", "Data records within reference entities."),
        ("Reference Entity Media File", None, "Binary media for reference entity records."),
    ]),
    ("AkeneoContext.Assets.cs", [
        ("Asset Family", "AssetFamily", "Asset family definitions (DAM-like)."),
        ("Asset Attribute", "AssetAttribute", "Attributes on asset families."),
        ("Asset Attribute Option", "AssetAttributeOption", "Options for asset select attributes."),
        ("Asset Media File", None, "Binary media for assets."),
        ("Asset", "Asset", "Individual asset records within families."),
    ]),
    ("AkeneoContext.Catalogs.cs", [
        ("Catalog", "Catalog", "Catalog for Apps — curated product selections."),
    ]),
    ("AkeneoContext.Jobs.cs", [
        ("Job", "Job", "Import/export job definitions and executions."),
    ]),
    ("AkeneoContext.Workflows.cs", [
        ("Workflow", "Workflow", "Enterprise workflow definitions (optional feature)."),
        ("Workflow Step Assignee", "WorkflowStepAssignee", "Users assigned to workflow steps."),
        ("Workflow Task", "WorkflowTask", "Individual workflow task instances."),
    ]),
    ("AkeneoContext.Utilities.cs", [
        ("System & Permissions", None, "System info, API overview, permissions, extensions, and modelization suggestions."),
    ]),
]


def group_key(method_name):
    """Map method name to its region/group."""
    name = method_name.lower()
    # Order matters — more specific first
    if "workflowtask" in name: return "Workflow Task"
    if "workflowstepassignee" in name: return "Workflow Step Assignee"
    if "workflow" in name: return "Workflow"
    if "catalogmappedvariant" in name: return "Catalog"
    if "catalogmappedmodel" in name: return "Catalog"
    if "catalogmappedproduct" in name: return "Catalog"
    if "catalogmappingschema" in name: return "Catalog"
    if "catalogproductuuid" in name: return "Catalog"
    if "catalogproduct" in name: return "Catalog"
    if "catalog" in name: return "Catalog"
    if "modelizationsuggestion" in name: return "System & Permissions"
    if "extension" in name: return "System & Permissions"
    if "systeminformation" in name: return "System & Permissions"
    if "apioverview" in name: return "System & Permissions"
    if "userchannel" in name: return "System & Permissions"
    if "userlocale" in name: return "System & Permissions"
    if "jobexecution" in name: return "Job"
    if "launchexport" in name: return "Job"
    if "launchimport" in name: return "Job"
    if "job" in name: return "Job"
    if "assetattributeoption" in name: return "Asset Attribute Option"
    if "assetattribute" in name: return "Asset Attribute"
    if "assetmedia" in name: return "Asset Media File"
    if "assetfamily" in name: return "Asset Family"
    if "asset" in name: return "Asset"
    if "referenceentityattributeoption" in name: return "Reference Entity Attribute Option"
    if "referenceentityattribute" in name: return "Reference Entity Attribute"
    if "referenceentityrecord" in name: return "Reference Entity Record"
    if "referenceentitymedia" in name: return "Reference Entity Media File"
    if "referenceentity" in name: return "Reference Entity"
    if "measurementfamil" in name: return "Measurement Family"
    if "currency" in name or "currencies" in name: return "Currency"
    if "locale" in name: return "Locale"
    if "channel" in name: return "Channel"
    if "associationtype" in name: return "Association Type"
    if "familyvariant" in name: return "Family Variant"
    if "family" in name: return "Family"
    if "categorymedia" in name: return "Category"
    if "category" in name or "categories" in name: return "Category"
    if "attributegroup" in name: return "Attribute Group"
    if "attributeoption" in name: return "Attribute Option"
    if "attribute" in name: return "Attribute"
    if "productmedia" in name: return "Product Media File"
    if "productmodel" in name: return "Product Model"
    if "productidentifier" in name: return "Product Identifier"
    if "productuuid" in name: return "Product UUID"
    if "searchproductuuid" in name: return "Product UUID"
    return "Other"


# ── Build the reference file ────────────────────────────────────────────────

def build_reference():
    lines = []
    w = lines.append

    w("# OpenAkeneo .NET Client — API Reference")
    w("")
    w("Compact reference for the `AkeneoContext` class in `OpenAkeneo.RestApiClient`.")
    w("All methods are `async` and accept an optional `CancellationToken ct` (omitted below for brevity).")
    w("")
    w("- **Index file**: See `OpenAkeneo.RestApiClient.ApiIndex.md` for a line-number lookup table.")
    w("- **README**: See `README.md` for installation, DI setup, usage examples, and attribute value handling.")
    w("- **Akeneo API docs**: https://api.akeneo.com/api-reference.html")
    w("")

    # Common patterns section
    w("---")
    w("")
    w("## Common Patterns")
    w("")
    w("### Pagination")
    w("Most list resources offer three method variants:")
    w("| Pattern | Signature | Description |")
    w("|---------|-----------|-------------|")
    w("| **Stream** | `IAsyncEnumerable<T> Stream…Async(…)` | Lazily follows all pages. Best for large datasets. |")
    w("| **Full list** | `Task<List<T>> Get…ListFullAsync(…)` | Materialises every item into a `List<T>`. Convenience wrapper around Stream. |")
    w("| **Single page** | `Task<…List> Get…ListAsync(page, limit, …)` | Returns one page with HAL navigation links. |")
    w("")
    w("Common pagination parameters (not repeated per-method below):")
    w("- `page` (int): 1-based page number.")
    w("- `limit` (int): Results per page, 1–100.")
    w("- `withCount` (bool): Include total count (may impact perf on large catalogs).")
    w("")
    w("Some resources (reference entities, asset families) use **keyset/cursor pagination** via `searchAfter` instead of page numbers.")
    w("")
    w("### Error Handling")
    w("All methods throw `AkeneoApiException` on non-success HTTP responses.")
    w("| Property | Type | Description |")
    w("|----------|------|-------------|")
    w("| `StatusCode` | `HttpStatusCode` | HTTP status code (401, 403, 404, 422, etc.) |")
    w("| `ApiMessage` | `string` | Akeneo error message from response body |")
    w("| `RequestUrl` | `string` | The URL that failed |")
    w("| `RequestMethod` | `string` | HTTP method used |")
    w("| `ResponseBody` | `string?` | Full response body for debugging |")
    w("")
    w("Common HTTP status codes:")
    w("- `401` — Authentication required (missing/expired token).")
    w("- `403` — Insufficient permissions or payload too large.")
    w("- `404` — Resource not found.")
    w("- `422` — Validation error in request body.")
    w("")
    w("### Create-or-Update (Upsert)")
    w("`CreateOrUpdate…Async()` methods use HTTP PATCH. Akeneo returns 201 on creation (body deserialized directly) or 204 on update (followed by a GET to return the current state).")
    w("")
    w("### Dictionary Overloads")
    w("Most `Get…ListAsync` methods have an additional overload accepting `Dictionary<string, string> queryParameters` for advanced/custom query strings not covered by the typed parameters.")
    w("")

    # Attribute value types
    w("### Attribute Value Types")
    w("Product/model `values` follow this structure: `{ \"attr_code\": [{ locale, scope, data }] }`")
    w("")
    w("| Attribute Type | `data` Shape |")
    w("|----------------|-------------|")
    w("| `pim_catalog_text`, `pim_catalog_textarea`, `pim_catalog_identifier` | `string` |")
    w("| `pim_catalog_boolean` | `bool` |")
    w("| `pim_catalog_number` | `string` (numeric) |")
    w("| `pim_catalog_date` | `string` (ISO 8601) |")
    w("| `pim_catalog_simpleselect` | `string` (option code) |")
    w("| `pim_catalog_multiselect` | `string[]` (option codes) |")
    w("| `pim_catalog_metric` | `{ amount: string, unit: string }` |")
    w("| `pim_catalog_price_collection` | `[{ amount: string, currency: string }]` |")
    w("| `pim_catalog_image`, `pim_catalog_file` | `string` (media file code) |")
    w("| `pim_catalog_asset_collection` | `string[]` (asset codes) |")
    w("| `pim_catalog_table` | `[{ column_code: value, … }]` |")
    w("| `akeneo_reference_entity`, `akeneo_reference_entity_collection` | `string` / `string[]` (record codes) |")
    w("")

    w("---")
    w("")

    # Now emit each resource group
    all_methods = {}
    for filename, groups in FILES_AND_GROUPS:
        filepath = os.path.join(SRC, filename)
        if not os.path.exists(filepath):
            continue
        parsed = parse_file(filepath)
        for method in parsed:
            gk = group_key(method["name"])
            all_methods.setdefault(gk, []).append(method)

    for filename, groups in FILES_AND_GROUPS:
        for group_name, model_type, group_desc in groups:
            methods = all_methods.get(group_name, [])
            if not methods:
                continue

            w(f"## {group_name}")
            w(f"{group_desc}")
            if model_type:
                w(f"**Model:** `{model_type}`")
            w("")

            for method in methods:
                # Method signature
                ret = method["return_type"]
                params_str = ", ".join(
                    f"{method['param_types'].get(p[0], '?')} {p[0]}"
                    for p in method["params"]
                )
                w(f"### `{method['name']}({params_str})`")
                w(f"**Returns:** `{ret}`")
                # Combine summary and return desc, skip empty/generic ones
                summary = method['summary']
                ret_desc = method['return_desc']
                if ret_desc and ret_desc not in summary and len(ret_desc) > 10:
                    w(f"{summary} {ret_desc}")
                else:
                    w(f"{summary}")
                if method["params"]:
                    w("**Parameters:**")
                    for pname, pdesc in method["params"]:
                        ptype = method["param_types"].get(pname, "")
                        w(f"- `{pname}` ({ptype}): {pdesc}")
                w("")

    # Schemas section — emit key schemas with structure hints
    w("---")
    w("")
    w("## Key Schemas")
    w("")
    w("These are the primary .NET model types used as method parameters and return values.")
    w("All models use `System.Text.Json` serialization with `JsonPropertyName` attributes mapping to Akeneo's snake_case JSON.")
    w("")

    schemas = [
        ("ProductUuid", [
            ("Uuid", "string", "Product UUID"),
            ("Enabled", "bool", "Whether the product is enabled"),
            ("Family", "string?", "Family code"),
            ("Categories", "List<string>", "Category codes"),
            ("Groups", "List<string>", "Group codes"),
            ("Parent", "string?", "Parent product model code"),
            ("Values", "Dictionary<string, List<ProductValue>>", '`{ "attr_code": [{ Locale, Scope, Data, LinkedData }] }`'),
            ("Associations", "Dictionary<string, Association>", '`{ "assoc_type": { Products, ProductModels, Groups } }`'),
            ("QuantifiedAssociations", "Dictionary<string, QuantifiedAssociation>", '`{ "assoc_type": { Products: [{ Uuid, Quantity }] } }`'),
            ("Created", "string", "ISO 8601 timestamp"),
            ("Updated", "string", "ISO 8601 timestamp"),
            ("Metadata", "JsonElement?", "Workflow status, etc."),
            ("QualityScores", "List<QualityScore>?", "Per-channel/locale quality scores"),
            ("Completenesses", "List<Completeness>?", "Per-channel/locale completeness"),
        ]),
        ("ProductIdentifier", [
            ("Identifier", "string", "Product SKU"),
            ("Uuid", "string?", "Product UUID (also available on legacy endpoint)"),
            ("Enabled", "bool", "Whether the product is enabled"),
            ("Family", "string?", "Family code"),
            ("Categories", "List<string>", "Category codes"),
            ("Values", "Dictionary<string, List<ProductValue>>", "Same structure as ProductUuid.Values"),
        ]),
        ("ProductModel", [
            ("Code", "string", "Product model code"),
            ("Family", "string?", "Family code"),
            ("FamilyVariant", "string", "Family variant code"),
            ("Parent", "string?", "Parent model code"),
            ("Categories", "List<string>", "Category codes"),
            ("Values", "Dictionary<string, List<ProductValue>>", "Attribute values"),
        ]),
        ("AkeneoAttribute", [
            ("Code", "string", "Attribute code"),
            ("Type", "string", "Attribute type (pim_catalog_text, etc.)"),
            ("Labels", "Dictionary<string, string>", "Localized labels"),
            ("Group", "string", "Attribute group code"),
            ("Localizable", "bool", "Whether values vary by locale"),
            ("Scopable", "bool", "Whether values vary by channel"),
            ("Unique", "bool", "Whether values must be unique"),
        ]),
        ("AttributeOption", [
            ("Code", "string", "Option code"),
            ("Attribute", "string", "Parent attribute code"),
            ("SortOrder", "int", "Display order"),
            ("Labels", "Dictionary<string, string>", "Localized labels"),
        ]),
        ("Family", [
            ("Code", "string", "Family code"),
            ("AttributeAsLabel", "string", "Attribute used as product label"),
            ("AttributeAsImage", "string?", "Attribute used as product image"),
            ("Attributes", "List<string>", "Attribute codes in this family"),
            ("AttributeRequirements", "Dictionary<string, List<string>>", "Required attrs per channel"),
            ("Labels", "Dictionary<string, string>", "Localized labels"),
        ]),
        ("Category", [
            ("Code", "string", "Category code"),
            ("Parent", "string?", "Parent category code"),
            ("Labels", "Dictionary<string, string>", "Localized labels"),
            ("Position", "int", "Sort position in tree"),
        ]),
        ("Channel", [
            ("Code", "string", "Channel code"),
            ("Locales", "List<string>", "Enabled locale codes"),
            ("Currencies", "List<string>", "Enabled currency codes"),
            ("CategoryTree", "string", "Root category code"),
            ("Labels", "Dictionary<string, string>", "Localized labels"),
        ]),
        ("ReferenceEntity", [
            ("Code", "string", "Entity code"),
            ("Labels", "Dictionary<string, string>", "Localized labels"),
            ("Image", "string?", "Image media file code"),
        ]),
        ("ReferenceEntityRecord", [
            ("Code", "string", "Record code"),
            ("Values", "Dictionary<string, List<RecordValue>>", "Attribute values"),
        ]),
        ("AssetFamily", [
            ("Code", "string", "Family code"),
            ("Labels", "Dictionary<string, string>", "Localized labels"),
            ("AttributeAsMainMedia", "string?", "Main media attribute code"),
        ]),
        ("Asset", [
            ("Code", "string", "Asset code"),
            ("AssetFamilyCode", "string", "Parent family code"),
            ("Values", "Dictionary<string, List<AssetValue>>", "Attribute values"),
        ]),
        ("Catalog", [
            ("Id", "string", "Catalog UUID"),
            ("Name", "string", "Display name"),
            ("Enabled", "bool", "Whether the catalog is active"),
        ]),
        ("ProductMediaFile", [
            ("Code", "string", "Media file code"),
            ("OriginalFilename", "string", "Original file name"),
            ("MimeType", "string", "MIME type"),
            ("Size", "int", "File size in bytes"),
            ("Extension", "string", "File extension"),
        ]),
    ]

    for schema_name, fields in schemas:
        w(f"### {schema_name}")
        w("| Property | Type | Description |")
        w("|----------|------|-------------|")
        for fname, ftype, fdesc in fields:
            w(f"| `{fname}` | `{ftype}` | {fdesc} |")
        w("")

    return "\n".join(lines)


# ── Build the index file ────────────────────────────────────────────────────

def build_index(reference_content):
    ref_lines = reference_content.split("\n")

    # Build a line-number map for headings
    heading_map = []
    for i, line in enumerate(ref_lines):
        if line.startswith("## ") or line.startswith("### "):
            heading_map.append((i + 1, line.strip()))

    lines = []
    w = lines.append

    w("# OpenAkeneo .NET Client — API Index")
    w("")
    w("Lookup index for `OpenAkeneo.RestApiClient.ApiReference.md`.")
    w("Each entry shows the .NET method or section and its line range in the Reference file.")
    w("Use this to find what you need, then read those lines from the Reference for full details.")
    w("")
    w("**Companion files:**")
    w("- **Reference**: `OpenAkeneo.RestApiClient.ApiReference.md` — full method signatures, parameters, return types, and schemas.")
    w("- **README**: `README.md` — installation, DI configuration, usage examples, and attribute value patterns.")
    w("")

    w("---")
    w("")
    w("## Common Patterns & Conventions")
    w("")
    w("All methods live on `AkeneoContext` (high-level client). Key conventions:")
    w("")
    w("- **Async-only**: Every method is `async` and returns `Task<T>` or `IAsyncEnumerable<T>`.")
    w("- **CancellationToken**: All methods accept an optional `CancellationToken ct` parameter (omitted in listings below).")
    w("- **Pagination triple**: Most resources have `Stream…Async` (lazy all-pages), `Get…ListFullAsync` (materialised list), `Get…ListAsync` (single page).")
    w("- **Upsert**: `CreateOrUpdate…Async` uses PATCH — creates on first call, updates on subsequent calls.")
    w("- **Error handling**: All methods throw `AkeneoApiException` with `StatusCode`, `ApiMessage`, `RequestUrl`, `ResponseBody` properties.")
    w("- **Attribute values**: Product/model `Values` are `Dictionary<string, List<ProductValue>>` where each value has `Locale`, `Scope`, `Data`.")
    w("- **Dictionary overloads**: Most list methods have an overload accepting `Dictionary<string, string>` for custom query parameters.")
    w("")
    w("See Reference Lines 1–70 for the full Common Patterns section including attribute value type mappings.")
    w("")

    w("---")
    w("")

    # Now build the index by resource group
    # Find ## headings (resource groups) and ### headings (methods)
    current_group = None
    current_group_line = 0
    group_entries = []

    for i, (line_num, heading) in enumerate(heading_map):
        if heading.startswith("## "):
            # Flush previous group
            if current_group and group_entries:
                # Find end of group
                next_group_line = heading_map[i][0] if i < len(heading_map) else len(ref_lines)
                w(f"### {current_group}")
                w(f"Lines {current_group_line}–{next_group_line - 2}")
                w("")
                for entry_line, entry_name in group_entries:
                    # Find end line for this entry
                    end_line = next_group_line - 2
                    for j, (el, _) in enumerate(heading_map):
                        if el > entry_line:
                            end_line = el - 2
                            break
                    w(f"- {entry_name}: L{entry_line}–{end_line}")
                w("")

            title = heading[3:].strip()
            current_group = title
            current_group_line = line_num
            group_entries = []
        elif heading.startswith("### "):
            title = heading[4:].strip()
            group_entries.append((line_num, title))

    # Flush last group
    if current_group and group_entries:
        end = len(ref_lines)
        w(f"### {current_group}")
        w(f"Lines {current_group_line}–{end}")
        w("")
        for entry_line, entry_name in group_entries:
            end_line = end
            for el, _ in heading_map:
                if el > entry_line:
                    end_line = el - 2
                    break
            w(f"- {entry_name}: L{entry_line}–{end_line}")
        w("")

    return "\n".join(lines)


# ── Main ─────────────────────────────────────────────────────────────────────

if __name__ == "__main__":
    print("Generating API Reference...")
    ref_content = build_reference()

    ref_path = os.path.join(os.path.dirname(__file__), "OpenAkeneo.RestApiClient.ApiReference.md")
    with open(ref_path, "w", encoding="utf-8", newline="\n") as f:
        f.write(ref_content)
    print(f"  Written {len(ref_content):,} bytes to {ref_path}")
    print(f"  {ref_content.count(chr(10))+1} lines")

    print("Generating API Index...")
    idx_content = build_index(ref_content)

    idx_path = os.path.join(os.path.dirname(__file__), "OpenAkeneo.RestApiClient.ApiIndex.md")
    with open(idx_path, "w", encoding="utf-8", newline="\n") as f:
        f.write(idx_content)
    print(f"  Written {len(idx_content):,} bytes to {idx_path}")
    print(f"  {idx_content.count(chr(10))+1} lines")

    print("Done!")
