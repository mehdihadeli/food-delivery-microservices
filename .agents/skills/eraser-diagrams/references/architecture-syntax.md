# Architecture diagram syntax

## Nodes

A node is the most basic building block in a cloud architecture diagram.

Node definitions consist of a name followed by an optional set of properties. For example, `compute` is the name of below node and it has an `icon` property which is set to `aws-ec2`.

```
compute [icon: aws-ec2]
```

Node names are required to be unique.

Nodes support `icon` and `color` properties.

## Groups

A group is a container that can encapsulate nodes and groups.

Group definitions consist of a name followed by `{ }`. For example, `Main Server` is the name of the below group and it contains `Server` and `Data` nodes.

```
  Main Server {
    Server [icon: aws-ec2]
    Data [icon: aws-rds]
  }
```

Group names are required to be unique.

Groups can be nested. In the below example, `VPC Subnet` group contains `Main Server` group.

```
VPC Subnet {
  Main Server {
    Server [icon: aws-ec2]
    Data [icon: aws-rds]
  }
}
```

Groups support `icon` and `color` properties.

## Properties

Properties are key-value pairs enclosed in `[ ]` brackets that can be appended to definitions of nodes and groups. Properties are optional.

It is possible to set multiple properties like shown below:

```
  Main Server [icon: aws-ec2, color: blue] {
    Server [icon: aws-ec2]
    Data [icon: aws-rds]
  }
```

Here are the properties that are allowed:

| Property | Description | Value | Default value |
|----------|-------------|-------|---------------|
| icon | Attached icons | Icon names (e.g. aws-ec2). | |
| color | Stroke and fill color | Color name (e.g. blue) or hex code (e.g. "#000000"- note the quote marks for hex codes) | |
| label | Text label | Any string. Enclose in double quotes (e.g. "Main Server") if containing a space. Allows multiple nodes and groups to have the same label. | Name of node or group |
| link | Internal or external link | A fully fledged URL. Enclose in double quotes (e.g. "https://my-internal-docs.io/api-docs". Supports the full gamut of external links and Eraser-specific links: diagrams, headers, and files in Eraser. | |
| colorMode | Fill color lightness | pastel, bold, outline | pastel |
| styleMode | Embellishments | shadow, plain, watercolor | shadow |
| typeface | Text typeface | rough, clean, mono | rough |

The `label` property is useful if you want the node's (or group's) label and name to be distinct. By default, the `label` is set as the node name. But because node names are required to be distinct, you will need to use the `label` property if you have two nodes with the exact same label.

```
// Names need to be distinct, but labels can overlap
Server_A [label: server]
Server_B [label: server]
```

It is possible to set multiple properties by separating them using `,` like shown below:

```
Server [icon: server, typeface: mono]
```

## Connections

Connections represent relationships between nodes and groups. They can be created between nodes, between groups, and between nodes and groups.

Here is an example of a connection between two nodes:

```
Compute > Storage
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

It is possible to add a label to a connection. Here is an example:

```
Storage > Server: Cache Hit
```

It is possible to create one-to-many connections in a single statement. This is instead of creating separate one-to-one connections. Here is an example:

```
Server > Worker1, Worker2, Worker3
```

If a connection statement contains a name that has not been previously defined as a node or a group, a blank node with that name will be created.

Here are the properties that are allowed on connections (lines):

| Property | Description | Example |
|----------|-------------|---------|
| color | Line color | `Storage > Server: Cache Hit [color: green]` `Storage > Server: [color: green]` |

## Escape string

Certain characters are not allowed in node and group names because they are reserved. You can use these characters, you can wrap the entire node or group name in quotes `" "`.

```
User > "https://localhost:8080": GET
```

## Direction

The direction of the cloud architecture diagram can be changed using the `direction` statement. Allowed directions are:

- `direction down`
- `direction up`
- `direction right` (default)
- `direction left`

The direction statement can be placed anywhere in the code like this:

```
direction down
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

### AWS Diagram

```
// Define groups and nodes
API gateway [icon: aws-api-gateway]
Lambda [icon: aws-lambda]
S3 [icon: aws-simple-storage-service]
VPC Subnet {
  Main Server {
    Server [icon: aws-ec2]
    Data [icon: aws-rds]
  }
  Queue [icon: aws-auto-scaling]
  Compute Nodes {
    Worker1 [icon: aws-ec2]
    Worker2 [icon: aws-ec2]
    Worker3 [icon: aws-ec2]
  }
}
Analytics [icon: aws-redshift]

// Define connections
API gateway > Lambda > Server > Data
Server > Queue
Queue > Worker1, Worker2, Worker3
S3 < Data
Compute Nodes > Analytics
```

### Google Cloud Diagram

```
// Define groups and nodes
Stream [icon: kafka, color: grey]
Ingest {
  Pub/Sub [icon: gcp-pubsub]
  Logging [icon: gcp-cloud-logging]
}
Pipelines {
  Dataflow [icon: gcp-dataflow]
}
Storage [icon: gcp-cloud-storage] {
  Datastore [icon: gcp-datastore]
  Bigtable [icon: gcp-bigtable]
}
Analytics {
  BigQuery [icon: gcp-bigquery]
}
Application [icon: gcp-app-engine] {
  App Engine [icon: gcp-app-engine]
  Container Engine [icon: gcp-container-registry]
  Compute Engine [icon: gcp-compute-engine]
}

// Define connections
Stream > Ingest
Logging > Analytics > Application
Pub/Sub > Pipelines > Storage > Application
```

### Azure Diagram

```
// Define groups and nodes
AD tenant [icon: azure-active-directory]
Load Balancers [icon: azure-load-balancers]
Virtual Network [icon: azure-virtual-networks] {
  Web Tier [icon: azure-network-security-groups] {
    vm1 [icon: azure-virtual-machine]
    vm2 [icon: azure-virtual-machine]
    vm3 [icon: azure-virtual-machine]
  }
  Business Tier [icon: azure-network-security-groups] {
    lb2 [icon: azure-load-balancers]
    vm4 [icon: azure-virtual-machine]
    vm5 [icon: azure-virtual-machine]
    vm6 [icon: azure-virtual-machine]
  }
}

// Define connections
AD tenant > Load Balancers
Load Balancers > vm1, vm2, vm3
vm1, vm2, vm3 > lb2 > vm4, vm5, vm6
```

### Kubernetes Diagram

```
// Define groups and nodes
Cloud Provider API [icon: settings]
AWS [icon: aws]
GCP [icon: google-cloud]
Azure [icon: azure]
Control Plane [icon: k8s-control-plane]{
  api [icon: k8s-api]
  sched [icon: k8s-sched]
  ccm [icon: k8s-c-c-m]
  cm [icon: k8s-c-m]
  etcd [icon: k8s-etcd]
}
Node1 [icon: k8s-node] {
  kubelet1 [icon: k8s-kubelet]
  kproxy1 [icon: k8s-k-proxy]
}
Node2 [icon: k8s-node] {
  kubelet2 [icon: k8s-kubelet]
  kproxy2 [icon: k8s-k-proxy]
}
Node3 [icon: k8s-node] {
  kubelet3 [icon: k8s-kubelet]
  kproxy3 [icon: k8s-k-proxy]
}

// Define connections
ccm > Cloud Provider API
Cloud Provider API > AWS, Azure, GCP
api > ccm, sched, etcd, cm
kubelet1, kproxy1, kubelet2, kproxy2, kubelet3, kproxy3 > api
```

### Data ETL Pipeline

```
// Define groups and nodes
Input data sources {
  Oracle [icon: oracle]
  Twitter [icon: twitter]
  Facebook [icon: facebook]
}
ETL pipeline [color: silver]{
  User survey data [icon: kafka]
  Data load [icon: aws-s3]
  Data transformation [icon: databricks]
  Data store [icon: snowflake]
}
Data destinations {
  Notification [icon: slack]
  Experimentation [icon: tensorflow]
  BI dashboard [icon: tableau]
}

// Define connections
Oracle, Twitter, Facebook > User survey data
User survey data > Data load > Data transformation > Data store
Data store > Notification, Experimentation, BI dashboard
```
