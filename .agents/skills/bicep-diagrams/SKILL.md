---
name: bicep-diagrams
description: 'Generates architecture diagrams from Azure Bicep files. Use when user has .bicep files or asks to visualize Bicep infrastructure.'
license: MIT
compatibility: Requires network access to call Eraser API
allowed-tools: Read Write Bash(curl:*)
metadata:
  version: "1.0.0"
  author: Eraser Labs
  tags: bicep, azure, diagram, infrastructure, iac, azure-resources
---

# Bicep Diagram Generator

Generates architecture diagrams directly from Azure Bicep files. Bicep is a domain-specific language (DSL) for deploying Azure resources declaratively.

## When to Use

Activate this skill when:

- User has Bicep files (`.bicep`) and wants to visualize the infrastructure
- User asks to "diagram my Bicep" or "visualize this Bicep infrastructure"
- User mentions Bicep or Azure Bicep
- User wants to see the architecture of their Bicep-deployed resources

## How It Works

This skill generates Bicep-specific diagrams by parsing Bicep code and calling the Eraser API directly:

1. **Parse Bicep Files**: Identify resource declarations, modules, parameters, and outputs
2. **Extract Relationships**: Map dependencies, resource references, and module hierarchies
3. **Generate Eraser DSL**: Create Eraser DSL code from Bicep resources
4. **Call Eraser API**: Use `/api/render/elements` with `diagramType: "cloud-architecture-diagram"`

## Instructions

When the user provides Bicep code:

1. **Parse the Bicep**

   - Identify all `resource` declarations (Microsoft.Compute/virtualMachines, etc.)
   - Extract `module` declarations and their configurations
   - Note `param` and `output` definitions
   - Identify `var` variables and their usage

2. **Map Relationships**

   - Track resource dependencies (e.g., `dependsOn` or implicit dependencies)
   - Group resources by type (compute, networking, storage, etc.)
   - Identify VNets as containers for subnets and resources
   - Note Network Security Groups, Key Vaults, and other security resources

3. **Generate Eraser DSL** Convert Bicep resources to Eraser DSL:

   - **CRITICAL: Label Formatting Rules**
     - Labels MUST be on a single line - NEVER use newlines inside label attributes
     - Keep labels simple and readable - prefer separate labels over concatenating too much metadata
     - Format DSL with proper line breaks (one node/group per line, but labels stay on single lines)
     - If including metadata like CIDR blocks or VM sizes, include them in the same quoted label string: `[label: "VNet 10.0.0.0/16"]`

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

4. **Make the HTTP Request**

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

5. **Track Sources During Analysis**

   As you analyze Bicep files and resources to generate the diagram, track:

   - **Internal files**: Record each Bicep file path you read and what resources were extracted (e.g., `infra/main.bicep` - VNet and subnet definitions, `infra/sql.bicep` - SQL Database configuration)
   - **External references**: Note any documentation, examples, or URLs consulted (e.g., Azure Bicep documentation, Azure architecture best practices)
   - **Annotations**: For each source, note what it contributed to the diagram

6. **Handle the Response**

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

7. **Handle Modules**
   - If modules are used, show module boundaries
   - Include module parameters and outputs
   - Show how modules connect to main resources

## Bicep-Specific Tips

- **Show Resource Groups**: Bicep deployments target resource groups
- **VNets as Containers**: Show VNets containing subnets and resources
- **Include Dependencies**: Show `dependsOn` relationships
- **Module Structure**: If modules are used, show their boundaries
- **Parameters**: Note key parameters that affect resource configuration
- **Use Azure Icons**: Request Azure-specific styling

## Example: Bicep with Parameters and Modules

### User Input

```bicep
@description('The name of the Virtual Network')
param vnetName string = 'myVNet'
@description('The address prefix for the VNet')
param vnetAddressPrefix string = '10.0.0.0/16'
@description('The address prefix for the subnet')
param subnetAddressPrefix string = '10.0.1.0/24'
@description('VM size')
param vmSize string = 'Standard_B1s'

// Main VNet resource
resource virtualNetwork 'Microsoft.Network/virtualNetworks@2021-05-01' = {
  name: vnetName
  location: resourceGroup().location
  properties: {
    addressSpace: {
      addressPrefixes: [vnetAddressPrefix]
    }
    subnets: [
      {
        name: 'subnet1'
        properties: {
          addressPrefix: subnetAddressPrefix
        }
      }
    ]
  }
}

// VM resource with dependsOn
resource virtualMachine 'Microsoft.Compute/virtualMachines@2021-11-01' = {
  name: 'myVM'
  location: resourceGroup().location
  properties: {
    hardwareProfile: {
      vmSize: vmSize
    }
  }
  dependsOn: [virtualNetwork]
}

// Module usage
module storageModule './modules/storage.bicep' = {
  name: 'storage'
  params: {
    location: resourceGroup().location
  }
}
```

### Expected Behavior

1. Parses Bicep:

   - **Parameters**: vnetName, vnetAddressPrefix, subnetAddressPrefix, vmSize
   - **Resources**: VNet with subnet, VM with dependsOn relationship
   - **Module**: Storage module with parameters

2. Generates DSL showing Bicep-specific features:

   ```
   myVNet [label: "VNet 10.0.0.0/16"] {
     subnet1 [label: "Subnet 1 10.0.1.0/24"] {
       myVM [icon: azure-vm, label: "VM Standard_B1s"]
     }
   }

   storage-module [label: "Storage Module"] {
     storage-account [icon: azure-storage]
   }

   myVNet -> myVM
   ```

   **Important**: All label text must be on a single line within quotes. Bicep-specific: Show modules as containers, include `dependsOn` relationships, note parameter usage in resource configuration.

3. Calls `/api/render/elements` with `diagramType: "cloud-architecture-diagram"`

4. Calls `/api/render/elements` with `diagramType: "cloud-architecture-diagram"`

### Result

User receives a diagram showing:

- VNet as a container
- Subnet nested inside VNet
- VM in the subnet
- Dependency relationship shown
- Proper Azure styling
