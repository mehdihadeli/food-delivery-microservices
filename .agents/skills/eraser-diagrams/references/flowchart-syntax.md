# Flow chart syntax

## Nodes

A node is the most basic building block in a flow chart.

Node definitions consist of a name followed by an optional set of properties. For example, `Start` is the name of below node and it has an `shape` property which is set to `oval`.

```
Start [shape: oval]
```

Node names are required to be unique.

Nodes support `shape`, `icon`, `color`, and `label` properties. More on properties below.

## Groups

A group is a container that can encapsulate nodes and groups.

Group definitions consist of a name followed by `{ }`. For example, `Loop` is the name of the below group and it contains `Issue1`, `Issue2`, and `Issue3` nodes.

```
Loop {
  Issue1, Issue2, Issue3
}
```

Nodes (or groups) inside a group can be enumerated either with `,` or new lines as separators. Below results in the same but uses new lines to separate each node.

```
Loop {
  Issue1
  Issue2
  Issue3
}
```

Group names are required to be unique.

Groups can be nested. In the below example, the `Outer Loop` group contains the `Inner Loop` group.

```
Outer Loop {
	Inner Loop {
		Issue1
		Issue2    
	}
	Issue3
}
```

Groups support `icon`, `color`, and `label` properties.

## Properties

Properties are key-value pairs enclosed in `[ ]` brackets that can be appended to definitions of nodes and groups. Properties are optional.

Here are the properties that are allowed:

| Property | Description | Value | Default Value |
|----------|-------------|-------|---------------|
| shape | Shape of node | Shape names (e.g. diamond or oval). | rectangle |
| icon | Icon | Icon names (e.g. aws-ec2). | |
| color | Stroke and fill color | Color name (e.g. blue) or hex code (e.g. "#000000"- note: must be wrapped in quotes) | |
| label | Text label | Any string. Enclose in double quotes (e.g. "Main Server") if containing a space. Allows multiple nodes and groups to have the same label. | Name of node or group |
| link | Internal or external link | A fully fledged URL. Enclose in double quotes (e.g. "https://my-internal-docs.io/api-docs". Supports the full gamut of external links and Eraser-specific links: diagrams, headers, and files in Eraser. | |
| colorMode | Fill color lightness | pastel, bold, outline | pastel |
| styleMode | Embellishments | shadow, plain, watercolor | shadow |
| typeface | Text typeface | rough, clean, mono | rough |

Here is the list of shapes:

- `rectangle` (default), `cylinder`, `diamond`, `document`, `ellipse`, `hexagon`, `oval`, `parallelogram`, `star`, `trapezoid`, `triangle`

The `label` property is useful if you want the node's (or group's) label and name to be distinct. By default, the `label` is set as the node name. But because node names are required to be distinct, you will need to use the `label` property if you have two nodes with the exact same label.

```
// Names need to be distinct, but labels can overlap
Start_A [label: start]
Start_B [label: start]
```

It is possible to set multiple properties by separating them using `,` like shown below:

```
Start [shape: oval, icon: flag]
```

## Connections

Connections represent relationships between nodes and groups. They can be created between nodes, between groups, and between nodes and groups.

Here is an example of a connection between two nodes:

```
Issue > Bug
```

Here are the types of connectors:

| Connector | Syntax | Description |
|-----------|--------|-------------|
| | `>` | Left-to-right arrow |
| | `<` | Right-to-left arrow |
| | `<>` | Bi-directional arrow |
| | `-` | Line |
| | `--` | Dotted line |
| | `-->` | Dotted arrow |

### Connection label

It is possible to add a label to a connection. Here is an example:

```
Issue > Bug: Triage
```

### Branching connections

It is possible to create one-to-many connections in a single statement. Here is an example:

```
Issue > Bug, Feature
```

### Chained connections

It is also possible to "chain" a sequence of connection statements in a single statement

```
Issue > Bug > Duplicate?
```

If a connection statement contains a name that has not been previously defined as a node or a group, a blank node with that name will be created.

### Connection properties

Here are the properties that are allowed on connections (lines):

| Property | Description | Example |
|----------|-------------|---------|
| color | Line color | `Issue > Bug: Triage [color: green]` `Issue > Bug: [color: green]` |

## Escape string

Certain characters are not allowed in node and group names because they are reserved. You can use these characters, you can wrap the entire node or group name in quotes `" "`.

```
User > "https://localhost:8080": GET
```

## Direction

The direction of the flow chart can be changed using the `direction` statement. Allowed directions are:

- `direction down` (default)
- `direction up`
- `direction right`
- `direction left`

The direction statement can be placed anywhere in the code like this:

```
direction right
```

## Styling

Styles can be applied at the diagram level. Below is an overview of the options and syntax. 

| Property | Values | Default value | Syntax example |
|----------|--------|---------------|----------------|
| colorMode | pastel, bold, outline | pastel | colorMode bold |
| styleMode | shadow, plain, watercolor | shadow | styleMode shadow |
| typeface | rough, clean, mono | rough | typeface clean |

## Examples

Here are some examples of diagrams you can create.

### Issue Triage Flow

```
// Nodes and groups
Issue type? [shape: oval, icon: file-text]

BugPath [color: red] {
  Bug [icon: bug, color: red]
  Duplicate? [shape: diamond, icon: copy]
  Mark duplicate [shape: oval, icon: copy]
  Has repro? [shape: diamond, icon: repeat]
  Ask for repro [shape: oval, icon: repeat]
}

FeaturePath [color: green] {
  Feature [icon: zap, color: green]
  Well specced? [shape: diamond, icon: check-square]
  Can be package? [shape: diamond, icon: package]
  Define as package [shape: oval, icon: package]
}

Issue ready to claim [shape: oval, icon: send]

// Relationships
Issue type? > Bug
Bug > Duplicate?
Duplicate? > Mark duplicate: Yes
Duplicate? > Has repro?: No
Has repro? > Issue ready to claim: Yes
Has repro? > Ask for repro: No
Issue type? > Feature
Feature > Can be package?
Can be package? > Well specced?: No
Can be package? > Define as package: Yes
Well specced? > Issue ready to claim: Yes
```

### Price Lookup Flow

```
// Define nodes and relationships "A > B"
Start [shape: oval, icon: flag] > Read keywords from Excel [icon: excel]
Read keywords from Excel > Establish Amazon API connection [icon: amazon]
Establish Amazon API connection > Wait for user input [shape: diamond, icon: user]
Wait for user input > Search for keyword on Amazon[icon: search]: User selects keyword
Search for keyword on Amazon > Retrieve item price [icon: dollar-sign] 
Retrieve item price > Output result to Excel [icon: excel] 
Output result to Excel > End
Wait for user input > Close modal [icon: x]: User clicks cancel
Close modal > End [shape: oval, icon: check]

// Define Groups
For each keyword in the list [icon: repeat] {
  Search for keyword on Amazon
  Retrieve item price 
  Output result to Excel
}
```
