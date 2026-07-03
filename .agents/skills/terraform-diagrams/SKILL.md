---
name: terraform-diagrams
description: 'Generates architecture diagrams from Terraform code. Use when user has .tf files or asks to visualize Terraform infrastructure.'
license: MIT
compatibility: Requires network access to call Eraser API
allowed-tools: Read Write Bash(curl:*)
metadata:
  version: "1.0.0"
  author: Eraser Labs
  tags: terraform, diagram, infrastructure, aws, azure, gcp, iac, hcl
---

# Terraform Diagram Generator

Generates architecture diagrams directly from Terraform `.tf` files. Specializes in parsing Terraform code and visualizing infrastructure resources, modules, and their relationships.

## When to Use

Activate this skill when:

- User has Terraform files (`.tf`, `.tfvars`) and wants to visualize the infrastructure
- User asks to "diagram my Terraform" or "visualize this infrastructure"
- User mentions Terraform, HCL, or infrastructure-as-code
- User wants to see the architecture of their Terraform-managed resources

## How It Works

This skill generates Terraform-specific diagrams by parsing Terraform code and calling the Eraser API directly:

1. **Parse Terraform Files**: Identify resources, modules, data sources, and variables
2. **Extract Relationships**: Map dependencies, resource connections, and module hierarchies
3. **Generate Eraser DSL**: Create Eraser DSL code from Terraform resources
4. **Call Eraser API**: Use `/api/render/elements` with `diagramType: "cloud-architecture-diagram"`

## Instructions

When the user provides Terraform code:

1. **Parse the Terraform**

   - Identify all `resource` blocks (AWS, Azure, GCP, etc.)
   - Extract `module` blocks and their configurations
   - Note `data` sources and their dependencies
   - Identify `variable` and `output` definitions

2. **Map Relationships**

   - Track resource dependencies (e.g., `subnet_id = aws_subnet.public.id`)
   - Group resources by provider (AWS, Azure, GCP)
   - Identify VPCs/VNets as containers for other resources
   - Note security groups, IAM roles, and networking rules

3. **Generate Eraser DSL** Convert Terraform resources to Eraser DSL:

   - **CRITICAL: Label Formatting Rules**
     - Labels MUST be on a single line - NEVER use newlines inside label attributes
     - Keep labels simple and readable - prefer separate labels over concatenating too much metadata
     - Format DSL with proper line breaks (one node/group per line, but labels stay on single lines)
     - If including metadata like CIDR blocks or instance types, include them in the same quoted label string: `[label: "VPC 10.0.0.0/16"]`

   Example:

   ```
   main-vpc [label: "VPC 10.0.0.0/16"] {
     public-subnet [label: "Public Subnet 10.0.1.0/24"] {
       web-server [icon: aws-ec2, label: "Web Server t3.micro"]
       load-balancer [icon: aws-elb]
     }
     private-subnet [label: "Private Subnet"] {
       database [icon: aws-rds]
     }
   }
   load-balancer -> web-server
   web-server -> database
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

   As you analyze Terraform files and resources to generate the diagram, track:

   - **Internal files**: Record each Terraform file path you read and what resources were extracted (e.g., `infra/main.tf` - VPC and subnet definitions, `infra/rds.tf` - Database configuration)
   - **External references**: Note any documentation, examples, or URLs consulted (e.g., Terraform AWS provider documentation, AWS architecture best practices)
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

7. **Handle Multiple Providers**
   - If Terraform uses multiple providers, group by provider
   - Create separate sections for AWS, Azure, GCP resources
   - Show cross-provider connections if applicable

## Terraform-Specific Tips

- **Group by Module**: If modules are used, show module boundaries
- **Show VPCs/VNets as Containers**: These should visually contain subnets and resources
- **Include Data Flows**: Show how resources connect (e.g., ALB → EC2 → RDS)
- **Highlight Security**: Include security groups, IAM roles, and network ACLs
- **Show Resource Types**: Use provider-specific icons (AWS, Azure, GCP)
- **Include CIDR Blocks**: Show network addressing for VPCs and subnets

## Example: Multi-Provider Terraform

### User Input

```hcl
# AWS Resources
resource "aws_vpc" "main" {
  cidr_block = "10.0.0.0/16"
}

resource "aws_subnet" "public" {
  vpc_id     = aws_vpc.main.id
  cidr_block = "10.0.1.0/24"
}

resource "aws_instance" "web" {
  subnet_id     = aws_subnet.public.id
  instance_type = "t3.micro"
}

# Azure Resources (multi-provider)
resource "azurerm_resource_group" "main" {
  name     = "rg-main"
  location = "East US"
}

resource "azurerm_virtual_network" "main" {
  name                = "vnet-main"
  resource_group_name  = azurerm_resource_group.main.name
  address_space        = ["10.1.0.0/16"]
}

# Module usage
module "database" {
  source = "./modules/rds"
  vpc_id = aws_vpc.main.id
}
```

### Expected Behavior

1. Parses Terraform:

   - **AWS**: VPC, subnet, EC2 instance
   - **Azure**: Resource group, VNet (multi-provider setup)
   - **Module**: Database module with dependency on VPC

2. Generates DSL showing multi-provider and module structure:

   ```
   # AWS Resources
   aws-vpc [label: "AWS VPC 10.0.0.0/16"] {
     aws-subnet [label: "Public Subnet 10.0.1.0/24"] {
       web-server [icon: aws-ec2, label: "Web Server t3.micro"]
     }
   }

   # Azure Resources
   resource-group [label: "Resource Group rg-main"] {
     azure-vnet [label: "Azure VNet 10.1.0.0/16"]
   }

   # Module
   database-module [label: "Database Module"] {
     rds-instance [icon: aws-rds]
   }

   aws-vpc -> database-module
   ```

   **Important**: All label text must be on a single line within quotes. Terraform-specific: Show modules as containers, group by provider, include resource dependencies.

3. Calls `/api/render/elements` with `diagramType: "cloud-architecture-diagram"`

### Result

User receives a diagram showing:

- VPC as a container
- Public subnet nested inside VPC
- EC2 instance in the subnet
- Proper AWS styling
