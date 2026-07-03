# BPMN diagram syntax

## Flow objects

Flow objects are the most basic building blocks in a BPMN diagram.

Flow object definitions consist of a name followed by a set of properties including the `type` property which determines whether it is an `activity`, `event`, or `gateway`. If omitted, it defaults to `activity`.

```
Place order[type: activity]
Shipped[type: event]
Approved?[type: gateway]
```

Flow object names are required to be unique.

Flow objects support `type`, `icon`, `color`, and `label` properties. More on properties below.

## Pools and Lanes

A pool is the top-level container which usually maps to organizations or departments. A pool can contain flow objects as well as lanes.

A lane is a sub container inside a pool which usually maps to roles or sub-organizations.

Pool and lane definitions consist of a name followed by `{ }`. The outermost brackets are assumed to be pools and any inner nested brackets are assumed to be lanes. For example, `Online store` is the name of the pool, which contains a `Warehouse` lane, which in turn contains the `Place order`, `Shipped`, and `Approved?` flow objects.

```
Online store {
  Warehouse {
		Place order[type: activity]
		Shipped[type: event]
		Approved?[type: gateway]
	}
}
```

Pools and lanes support `icon`, `color`, and `label` properties.

## Properties

Properties are key-value pairs enclosed in `[ ]` brackets that can be appended to definitions of flow objects, pools, and lanes. Properties are optional.

Here are the properties that are allowed:

| Property | Description | Value | Default Value |
|----------|-------------|-------|---------------|
| type | Flow object type | activity, event, gateway | activity |
| icon | Icon | Icon names (e.g. user). | |
| color | Stroke and fill color | Color name (e.g. blue) or hex code (e.g. "#000000"- note: must be wrapped in quotes) | |
| label | Text label | Any string. Enclose in double quotes (e.g. "Acme Corp") if containing a space. Allows multiple flow objects, pools, and lanes to have the label. | Name of flow object, pool, lane |
| link | Internal or external link | A fully fledged URL. Enclose in double quotes (e.g. "https://my-internal-docs.io/api-docs". Supports the full gamut of external links and Eraser-specific links: diagrams, headers, and files in Eraser. | |
| colorMode | Fill color lightness | pastel, bold, outline | pastel |
| styleMode | Embellishments | shadow, plain, watercolor | shadow |
| typeface | Text typeface | rough, clean, mono | rough |

The `label` property is useful if you want multiple flow objects, pools, or lanes, to have the same label since names are required to be unique. By default, the label is set as the name.

```
// Names need to be distinct, but labels can overlap
Employee_A [label: employee]
Employee_B [label: employee]
```

It is possible to set multiple properties by separating them using `,` like shown below:

```
Place order [type: activity, icon: flag]
```

## Connections

Connections represent show how work progresses, messages travel, or data links between elements in the process.

Here is an example of a connection between two activities:

```
Open website > Place order
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
Open website > Place order: Browse
```

### Connection properties

Here are the properties that are allowed on connections (lines):

| Property | Description | Example |
|----------|-------------|---------|
| color | Line color | `Open website > Place order: Browse[color: blue]` `Open website > Place order: [color: blue]` |

## Escape string

Certain characters are not allowed in names because they are reserved. To use these characters, you can wrap the entire flow object, pool, or lane name in quotes `" "`.

```
Customer {
	Open website [type: activity]
  "Place / cancel order" [type: activity]
}
```

## Styling

Styles can be applied at the diagram level. Below is an overview of the options and syntax.

| Property | Values | Default value | Syntax example |
|----------|--------|---------------|----------------|
| colorMode | pastel, bold, outline | pastel | colorMode bold |
| styleMode | shadow, plain, watercolor | shadow | styleMode shadow |
| typeface | rough, clean, mono | rough | typeface clean |

## Examples

Here are some examples of BPMN diagrams you can create.

### Expense Submission and Approval Flow

```
title Expense Submission and Approval Flow

User [color: lightblue] {
  Login [icon: log-in]
  Access confirmed [type: event, icon: check-circle]
  Start new expense report [icon: file-plus]
  Upload receipts [icon: upload]
  Reimbursed [type: event, icon: dollar-sign]
  User rejected [type: event, label: "Rejected", icon: x-circle]
}

System [color: orange] {
  OCR extract date & amount [icon: calendar]
  Check amount [type: gateway, icon: filter]
  "Auto-approve?" [type: gateway, icon: check]
  "Auto-approved" [type: event, icon: thumbs-up]
  Send to manager [icon: send]
  Process reimbursement [icon: credit-card]
}

Manager [color: green] {
  Review expense [icon: eye]
  Approve? [type: gateway, icon: check]
  Approved [type: event, icon: thumbs-up]
  Rejected [type: event, icon: x-circle]
}

// Connections within User pool
Login > Access confirmed
Access confirmed > Start new expense report
Start new expense report > Upload receipts

// User to System (dashed message flow)
Upload receipts -- OCR extract date & amount : Receipts

// System pool
OCR extract date & amount > Check amount
Check amount > "Auto-approve?" : Amount < $500
Check amount --> Rejected : Amount > $500

// Auto-approve gateway
"Auto-approve?" > "Auto-approved" : Amount < $50
"Auto-approve?" > Send to manager : $51–$500

// Auto-approved to reimbursement
"Auto-approved" > Process reimbursement

// Send to manager to Manager pool (dashed message flow)
Send to manager -- Review expense : Expense for approval

// Manager pool
Review expense > Approve?
Approve? > Approved : Yes
Approve? > Rejected : No

// Approved to System (dashed message flow)
Approved --> Process reimbursement : Approval

// Process reimbursement to User pool (dashed message flow)
Process reimbursement --> Reimbursed : Reimbursement
Upload receipts > Reimbursed

// Rejected to User pool (dashed message flow)
Rejected --> Reimbursed : Notify user
Upload receipts > User rejected
```

### Product Returns Process

```
title Product Returns Process

Customer [color: teal] {
  Return requested [type: event, icon: mail]
  Ship or drop off item [icon: truck]
  Return complete [type: event, icon: check-circle]
  "Notified: Return not accepted" [type: event, icon: alert-triangle]
}

Company [color: blue] {
  Customer Service {
    Check return eligibility [icon: check-square]
    Eligible? [type: gateway, icon: x]
    Send return instructions [icon: send]
    "Notify: Return not accepted" [icon: x-circle]
  }
  Warehouse {
    Receive & inspect item [icon: package]
    Item OK? [type: gateway, icon: x]
    Offer partial refund or return item [icon: slash]
    Approve return & update stock [icon: database]
  }
  Finance {
    "Issue refund/store credit/replacement" [icon: credit-card]
    "Notify customer: Return complete" [icon: mail]
  }
}

// Connections

Return requested > Check return eligibility
Check return eligibility > Eligible?
Eligible? > Send return instructions : Yes
Eligible? > "Notify: Return not accepted" : No
"Notify: Return not accepted" --> "Notified: Return not accepted"
Return requested > Ship or drop off item
Send return instructions > Ship or drop off item
Ship or drop off item --> Receive & inspect item
Receive & inspect item > Item OK?
Item OK? > Approve return & update stock : Yes
Item OK? > Offer partial refund or return item : No
Offer partial refund or return item --> "Issue refund/store credit/replacement"
Approve return & update stock --> "Issue refund/store credit/replacement"
"Issue refund/store credit/replacement" > "Notify customer: Return complete"
"Notify customer: Return complete" --> Return complete
Ship or drop off item > Return complete
Ship or drop off item > "Notified: Return not accepted"
```
