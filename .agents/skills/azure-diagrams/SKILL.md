---
name: azure-diagrams
description: 'Visualizes Azure infrastructure from ARM templates, Azure CLI, or descriptions. Use when user has Azure resources to diagram.'
license: MIT
compatibility: Requires network access to call Eraser API
allowed-tools: Read Write Bash(curl:*)
metadata:
  version: "1.0.0"
  author: Eraser Labs
  tags: azure, diagram, arm, resource-group, vnet, vm, storage, infrastructure
---

# Azure Diagram Generator

Generates architecture diagrams for Azure infrastructure from ARM templates, Azure CLI output, or natural language descriptions.

## When to Use

Activate this skill when:

- User has ARM (Azure Resource Manager) templates (JSON)
- User provides Azure CLI output (e.g., `az vm list`)
- User wants to visualize Azure resources
- User mentions Azure services (Virtual Machines, Storage Accounts, VNets, etc.)
- User asks to "diagram my Azure infrastructure"

## How It Works

This skill generates Azure-specific diagrams by parsing Azure resources and calling the Eraser API directly:

1. **Parse Azure Resources**: Extract resources from ARM templates, CLI output, or descriptions
2. **Map Azure Relationships**: Identify Resource Groups, VNets, subnets, and service connections
3. **Generate Eraser DSL**: Create Eraser DSL code from Azure resources
4. **Call Eraser API**: Use `/api/render/elements` with `diagramType: "cloud-architecture-diagram"`

## Instructions

When the user provides Azure infrastructure information:

1. **Parse the Source**

   - **ARM Templates**: Extract `resources` array, identify types (Microsoft.Compute/virtualMachines, etc.)
   - **CLI Output**: Parse JSON output from `az` commands
   - **Description**: Identify Azure service names and relationships

2. **Identify Azure Components**

   - **Networking**: Virtual Networks (VNets), Subnets, Network Security Groups, Load Balancers
   - **Compute**: Virtual Machines, Virtual Machine Scale Sets, App Services, Functions
   - **Storage**: Storage Accounts, Blob Storage, File Shares
   - **Databases**: SQL Databases, Cosmos DB, Redis Cache
   - **Security**: Network Security Groups, Azure AD, Key Vault
   - **Load Balancing**: Application Gateway, Load Balancer, Traffic Manager
   - **Other**: Service Bus, Event Hubs, API Management

3. **Map Relationships**

   - VMs in subnets
   - Subnets in VNets
   - VNets in Resource Groups
   - Storage accounts accessed by VMs
   - Databases accessed by applications
   - Network Security Groups attached to subnets

4. **Generate Eraser DSL** Convert Azure resources to Eraser DSL:

   - **CRITICAL: Label Formatting Rules**
     - Labels MUST be on a single line - NEVER use newlines inside label attributes
     - Keep labels simple and readable - prefer separate labels over concatenating too much metadata
     - Format DSL with proper line breaks (one node/group per line, but labels stay on single lines)
     - If including metadata like CIDR blocks or instance types, include them in the same quoted label string: `[label: "VNet 10.0.0.0/16"]`

   Example:

   ```
   myVNet [label: "VNet 10.0.0.0/16"] {
     subnet1 [label: "Subnet 1"] {
       myVM [icon: azure-vm, label: "Virtual Machine"]
       gateway [icon: azure-app-gateway]
     }
     subnet2 [label: "Subnet 2"] {
       database [icon: azure-sql]
     }
   }
   storage [icon: azure-storage]
   myVNet -> myVM
   myVM -> database
   ```

5. **Make the HTTP Request**

   **IMPORTANT**: You MUST execute this curl command after generating the DSL. Never stop after generating DSL without making the API call.

   **CRITICAL**: In the `X-Skill-Source` header below, you MUST replace the value with your AI agent name:
   - If you are Claude (Anthropic), use: `claude`
   - If you are running in Cursor, use: `cursor`
   - If you are ChatGPT (OpenAI), use: `chatgpt`
   - If you are Gemini (Google), use: `gemini`
   - Otherwise, use your model/agent name in lowercase

   ```bash
   curl -X POST https://app.eraser.io/api/render/elements \
     -H "Content-Type: application/json" \
     -H "X-Skill-Source: eraser-skill" \
     -H "Authorization: Bearer ${ERASER_API_KEY}" \
     -d '{
       "elements": [{
         "type": "diagram",
         "id": "diagram-1",
         "code": "<your generated DSL>",
         "diagramType": "cloud-architecture-diagram"
       }],
       "scale": 2,
       "theme": "${ERASER_THEME:-dark}",
       "background": true
     }'
   ```

6. **Track Sources During Analysis**

   As you analyze files and resources to generate the diagram, track:

   - **Internal files**: Record each file path you read and what information was extracted (e.g., `infra/main.bicep` - VNet and subnet definitions)
   - **External references**: Note any documentation, examples, or URLs consulted (e.g., Azure architecture best practices documentation)
   - **Annotations**: For each source, note what it contributed to the diagram

7. **Handle the Response**

   **CRITICAL: Minimal Output Format**

   Your response MUST always include these elements with clear headers:

   1. **Diagram Preview**: Display with a header
      ```
      ## Diagram
      ![{Title}]({imageUrl})
      ```
      Use the ACTUAL `imageUrl` from the API response.

   2. **Editor Link**: Display with a header
      ```
      ## Open in Eraser
      [Edit this diagram in the Eraser editor]({createEraserFileUrl})
      ```
      Use the ACTUAL URL from the API response.

   3. **Sources section**: Brief list of files/resources analyzed (if applicable)
      ```
      ## Sources
      - `path/to/file` - What was extracted
      ```

   4. **Diagram Code section**: The Eraser DSL in a code block with `eraser` language tag
      ```
      ## Diagram Code
      ```eraser
      {DSL code here}
      ```
      ```

   5. **Learn More link**: `You can learn more about Eraser at https://docs.eraser.io/docs/using-ai-agent-integrations`

   **Additional content rules:**
   - If the user ONLY asked for a diagram, include NOTHING beyond the 5 elements above
   - If the user explicitly asked for more (e.g., "explain the architecture", "suggest improvements"), you may include that additional content
   - Never add unrequested sections like Overview, Security Considerations, Testing, etc.

   The default output should be SHORT. The diagram image speaks for itself.

## Azure-Specific Tips

- **Resource Groups**: Show Resource Groups as logical containers
- **VNets as Containers**: Always show VNets containing subnets and resources
- **Network Security Groups**: Include NSG rules and attachments
- **Subscriptions**: Note subscription context if provided
- **Data Flow**: Show traffic flow (Internet → Application Gateway → VM → SQL Database)
- **Use Azure Icons**: Request Azure-specific styling in the description

## Example: ARM Template with Multiple Azure Services

### User Input

```json
{
  "resources": [
    {
      "type": "Microsoft.Resources/resourceGroups",
      "name": "rg-main"
    },
    {
      "type": "Microsoft.Network/virtualNetworks",
      "name": "myVNet",
      "properties": {
        "addressSpace": {
          "addressPrefixes": ["10.0.0.0/16"]
        },
        "subnets": [
          {
            "name": "subnet1",
            "properties": {
              "addressPrefix": "10.0.1.0/24"
            }
          }
        ]
      }
    },
    {
      "type": "Microsoft.Compute/virtualMachines",
      "name": "myVM",
      "properties": {
        "hardwareProfile": {
          "vmSize": "Standard_B1s"
        }
      }
    },
    {
      "type": "Microsoft.Web/sites",
      "name": "myAppService",
      "properties": {
        "serverFarmId": "/subscriptions/.../serverfarms/myPlan"
      }
    },
    {
      "type": "Microsoft.Storage/storageAccounts",
      "name": "mystorageaccount"
    },
    {
      "type": "Microsoft.Sql/servers",
      "name": "mysqlserver",
      "properties": {
        "administratorLogin": "admin"
      }
    }
  ]
}
```

### Expected Behavior

1. Parses ARM template:

   - **Resource Group**: rg-main (container)
   - **Networking**: VNet with subnet
   - **Compute**: VM, App Service
   - **Storage**: Storage Account
   - **Database**: SQL Server

2. Generates DSL showing Azure service diversity:

   ```
   resource-group [label: "Resource Group rg-main"] {
     myVNet [label: "VNet 10.0.0.0/16"] {
       subnet1 [label: "Subnet 1 10.0.1.0/24"] {
         myVM [icon: azure-vm, label: "VM Standard_B1s"]
       }
     }
     myAppService [icon: azure-app-service, label: "App Service"]
     mystorageaccount [icon: azure-storage, label: "Storage Account"]
     mysqlserver [icon: azure-sql, label: "SQL Server"]
   }

   myAppService -> mystorageaccount
   myVM -> mysqlserver
   ```

   **Important**: All label text must be on a single line within quotes. Azure-specific: Show Resource Groups as containers, include App Services, Storage Accounts, and SQL databases with proper Azure icons.

3. Calls `/api/render/elements` with `diagramType: "cloud-architecture-diagram"`

## Example: Azure CLI Output

### User Input

```
User runs: az vm list --output json
Provides JSON output
```

### Expected Behavior

1. Parses JSON to extract:

   - VM names, sizes, states
   - Resource groups
   - Network interfaces
   - Storage accounts

2. Formats and calls API
