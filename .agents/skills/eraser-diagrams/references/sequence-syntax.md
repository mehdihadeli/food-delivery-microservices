# Sequence diagram syntax

Each line in a sequence diagram consists of two columns (i.e. entities), an arrow (i.e. direction of flow), and a message. The two columns are separated by the `>` arrow and the message is prepended with the `:`.

Here is an example:

```
Web App > DB: Start transaction
```

Here are the types of arrows:

| Arrow | Syntax | Description |
|-------|--------|-------------|
| | `>` | Left-to-right arrow |
| | `<` | Right-to-left arrow |
| | `<>` | Bi-directional arrow |
| | `-` | Line |
| | `--` | Dotted line |
| | `-->` | Dotted arrow |

Each line is parsed in sequential order from top to bottom and rendered in the diagram the same way.

Column names are required to be unique. If a line refers to a column name that hasn't been used in prior lines, a new column will be created.

## Properties

Properties are key-value pairs enclosed in `[ ]` brackets that can be appended to column names. Properties are optional.

It is possible to set multiple properties like shown below:

```
Web App [icon: monitor, color: blue] > DB [icon: database, color: green]: Start transaction 
```

Here are the properties that are allowed on columns:

| Property | Description | Value | Default value |
|----------|-------------|-------|---------------|
| icon | Attached icons | Icon names (e.g. aws-ec2). | |
| color | Stroke and fill color | Color name (e.g. blue) or hex code (e.g. #000000) | |
| label | Text label | Any string. Enclose in double quotes (e.g. "Main Server") if containing a space. Allows multiple columns to have the same label. | |
| link | Internal or external link | A fully fledged URL. Enclose in double quotes (e.g. "https://my-internal-docs.io/api-docs". Supports the full gamut of external links and Eraser-specific links: diagrams, headers, and files in Eraser. | |
| colorMode | Fill color lightness | pastel, bold, outline | pastel |
| styleMode | Embellishments | shadow, plain, watercolor | shadow |
| typeface | Text typeface | rough, clean, mono | rough |

The `label` property is useful if you want the column label and name to be distinct. By default, the `label` is set as the column name. But because column names are required to be distinct, you will need to use the `label` property if you have two column with the exact same label.

```
// Names need to be distinct, but labels can overlap
Server1 [label: server]
Server2 [label: server]
```

Here are the properties that are allowed on arrows (lines):

| Property | Description | Example |
|----------|-------------|---------|
| color | Line color | `Web App > DB: Start transaction [color: blue]` `Web App > DB: [color: blue]` |

## Blocks

Blocks are groupings of messages that represent control flow. They can be used to express loops, if-else logic, parallel processing, and break execution.

Block definitions consist of a block type followed by `{ }`. It can have an optional label property. For example, the below `opt` (optional) block has a label `if complete`. The block contains a single message from `Server` to `Client`.

```
opt [label: if complete] {
 Server > Client: Success
}
```

There are 5 block types, each representing a type of control flow.

| Type | Description |
|------|-------------|
| loop | Loop |
| alt (else) | Alternative |
| opt | Optional |
| par(and) | Parallel |
| break | Break |

It is possible to create connected blocks in the case of the `alt` (paired with `else`) and `par` (paired with `and`) blocks.

```
alt [label: if complete] {
 Server > Client: Success
}
else [label: if failed] {
 Server > Client: Failure
}
```

Here are all the block properties that are allowed:

| Property | Description | Value |
|----------|-------------|-------|
| label | Add a label to the block | Block label. Can be any string. |
| icon | Add an icon to the block label | Icon names (e.g. aws-ec2).|
| color | Specify a color for the block | Color name (e.g. blue) or hex code (e.g. #000000) |

## Activations

Activations represent the time during which a column (an actor or resource) is actively performing an action.

A pair of `activate` and `deactivate` statements define a single activation. The `activate` and `deactivate` keyword is followed by the column name.

```
Client > Server: Data request
activate Server
Server > Client: Return data
deactivate Server
```

## Escape string

Certain characters are not allowed in node and group names because they are reserved. You can use these characters, you can wrap the entire node or group name in quotes `" "`.

```
User > "https://localhost:8080": GET
```

## Styling

Styles can be applied at the diagram level. Below is an overview of the options and syntax.

| Property | Values | Default value | Syntax example |
|----------|--------|---------------|----------------|
| colorMode | pastel, bold, outline | pastel | colorMode bold |
| styleMode | shadow, plain, watercolor | shadow | styleMode shadow |
| typeface | rough, clean, mono | rough | typeface clean |
| autoNumber | on, nested, off | off | autoNumber on |

## Examples

Here are some examples of sequence diagrams you can create in Eraser:

### Web App Transaction Flow

```
Web App [icon: layout] > DB [icon: database]: Start transaction
Web App > Cloud Fx [icon: function]: Call function
Cloud Fx > API [icon: cloud-cog]: Create session
API > Cloud Fx: Session info
Cloud Fx > DB: Create tx record
Cloud Fx > API: Request access token
API > Cloud Fx: Access token
Cloud Fx > Web App: Token and transaction info
Web App > API: Complete transaction
alt [label: If successful]{
  API > Web App: Transaction confirmation
}
else [label: If failed]{
  API > Web App: Transaction cancellation
}
Web App > DB: Create tx record
Web App > API: Subscribe to transaction changes
activate API 
API > API: Ongoing events
API > Web App: Push events
deactivate API
```
