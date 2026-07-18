# Food Delivery EventCatalog

A living, event-driven architecture catalog for [Food Delivery Microservices](https://github.com/mehdihadeli/food-delivery-microservices) that documents all integration events, services, channels, and message contracts flowing through the platform. Built with [EventCatalog](https://www.eventcatalog.dev/).

The catalog is deployed automatically to GitHub Pages on every push to `main` that touches `docs/eventcatalog/`.

**Live catalog**: https://mehdihadeli.github.io/food-delivery-microservices/

## Tech Stack

| Tool                | Version | Purpose                           |
| ------------------- | ------- | --------------------------------- |
| EventCatalog        | latest  | Catalog generator                 |
| EventCatalog Linter | ^1.1    | Schema validation                 |
| Prettier            | ^3.8    | Markdown and JSON formatting      |
| Node.js / npm       | 24+     | Package manager and script runner |

## Prerequisites

- [Node.js](https://nodejs.org/en/download/) >= 24.0.0
- npm 10+ (or Bun 1.0+)

## Getting Started

```bash
cd docs/eventcatalog
npm install
npm run dev
```

The development server opens at [http://localhost:3000](http://localhost:3000) by default.

## Available Scripts

| Script             | Description                                    |
| ------------------ | ---------------------------------------------- |
| `npm run dev`      | Start the development server with live reload  |
| `npm run build`    | Build the production static site               |
| `npm run start`    | Serve the built production site locally        |
| `npm run preview`  | Preview the production build                   |
| `npm run generate` | Run EventCatalog generators                    |
| `npm run lint`     | Validate catalog files with the linter         |
| `npm run format`   | Format all files with Prettier                 |
| `npm run check`    | Run both linter and Prettier check in one step |

## Catalog Structure

```
docs/eventcatalog/
├── domains/
│   └── FoodDelivery/                 # Root bounded context
│       ├── subdomains/
│       │   ├── Catalogs/             # Products, suppliers, stock
│       │   ├── Customers/            # Customers, restock subscriptions
│       │   ├── Identity/             # Users, roles, auth state
│       │   ├── Orders/               # Order lifecycle (event-sourced)
│       │   └── ...                   # Billing, Carts, Restaurants, etc.
│       └── ubiquitous-language.mdx   # Shared domain vocabulary
├── channels/                          # RabbitMQ exchanges and queues
├── teams/                             # Team ownership definitions
├── users/                             # Contributor and owner profiles
├── public/                            # Static assets (logo, images)
├── eventcatalog.config.js             # Catalog configuration
├── eventcatalog.styles.css            # Theme overrides
└── .eventcatalogrc.cjs                # Linter rules
```

## How Events Are Documented

Integration events are defined as shared C# contracts in [src/Services/Shared/FoodDelivery.Services.Shared](https://github.com/mehdihadeli/food-delivery-microservices/tree/main/src/Services/Shared/FoodDelivery.Services.Shared) and mapped into EventCatalog as structured documentation.

Each documented event follows this pattern:

- **Location**: `domains/FoodDelivery/subdomains/<Subdomain>/services/<Service>/events/<EventName>/`
- **Frontmatter**: `index.mdx` containing `id`, `version`, `name`, `summary`, `schemaPath`, and `owners`
- **Payload schema**: `schema.json` with a JSON Schema describing the event payload, including base `Message` fields (`messageId`, `created`) and event-specific properties
- **Channel**: a matching entry under `channels/<exchange_name>/index.mdx` describing the RabbitMQ exchange, queue, and routing key

For example, `ProductCreatedV1` is produced by Catalogs and consumed by Customers. Its documentation lives in:

```
domains/FoodDelivery/subdomains/Catalogs/services/CatalogsService/events/ProductCreatedV1/
├── index.mdx
└── schema.json
```

The service definition lists what it `sends` and `receives`, allowing EventCatalog to render the event flow and node graphs automatically.

## Content Guide

### Adding a New Event

1. Identify the subdomain and service that owns the event.
2. Create a folder under the appropriate service, e.g.:

   ```
   domains/FoodDelivery/subdomains/Catalogs/services/CatalogsService/events/ProductPriceChanged/
   ```

3. Add an `index.mdx` file with the required front matter:

   ```yaml
   ---
   id: ProductPriceChanged
   version: 1.0.0
   name: Product Price Changed
   summary: Emitted when a product price is updated
   badges:
     - content: "Broker:RabbitMQ"
       backgroundColor: green
       textColor: white
       icon: ArrowPathIcon
   schemaPath: schema.json
   owners:
     - mehdihadeli
   ---
   ```

4. Document the event payload with a JSON Schema in `schema.json`.
5. Register the event in the service's `sends` or `receives` list.
6. Add or update the corresponding `channels/<message_name_underscore>.primary_exchange/index.mdx` entry.

### Adding a New Service

1. Create a folder under the relevant subdomain, e.g.:

   ```
   domains/FoodDelivery/subdomains/Search/services/SearchService/
   ```

2. Add an `index.mdx` with `id`, `name`, `version`, `summary`, `sends`, and `receives` fields.
3. Link the service to the owning team and repository.

### Adding a Channel

1. Create a folder under `channels/`, e.g.:

   ```
   channels/product_price_changed_v1.primary_exchange/
   ```

2. Add an `index.mdx` describing the RabbitMQ exchange name, protocol (`amqp`), and address.

### Future Documentation Guidelines

As the system evolves, every new cross-service integration event **must** be documented in this catalog before the corresponding producer code is merged. Follow these rules:

- Keep event definitions co-located with the service that owns them.
- Use `schema.json` for every event; do not rely only on prose descriptions.
- Update both `sends` (producer) and `receives` (consumer) service frontmatter so EventCatalog can render accurate flow diagrams.
- Use consistent naming: event class names end with `V1`, `V2`, etc., and channel exchanges follow the pattern `<event_name_underscore>.primary_exchange`.
- Add channels for every published event, matching the topology created by `BuildingBlocks.Integration.Wolverine`.
- Run `npm run check` in pull requests to catch schema validation and formatting issues early.
- When a service starts consuming a new event, duplicate or reference the event documentation under the consumer service so the relationship is visible.

### Linting

Run `npm run lint` before opening a pull request to catch schema validation errors early. Run `npm run check` to verify both linting and formatting.

## Deployment

The catalog is deployed automatically to GitHub Pages when changes inside `docs/eventcatalog/` are pushed to `main`.

The deployment workflow is defined in [.github/workflows/eventcatalog.yml](../../.github/workflows/eventcatalog.yml). It:

1. Checks out the repository
2. Sets up Node.js
3. Installs dependencies from `docs/eventcatalog/`
4. Builds the static site with the correct `BASE` path for GitHub Pages
5. Uploads the `dist` folder as a Pages artifact
6. Deploys to GitHub Pages

To build and preview locally:

```bash
npm run build
npm run preview
```

If you use a custom domain instead of the default `https://mehdihadeli.github.io/food-delivery-microservices/`, update the `BASE` environment variable in the workflow to `/` and configure the domain in the repository's GitHub Pages settings.

## Related Resources

- 🌐 [Live Catalog](https://fooddelivery-dev.netlify.app)
- 📚 [Main Repository](https://github.com/mehdihadeli/food-delivery-microservices)
- 📘 [EventCatalog Docs](https://www.eventcatalog.dev/docs)
- 🐰 [RabbitMQ Topologies](https://github.com/mehdihadeli/food-delivery-microservices/tree/main/src/BuildingBlocks/BuildingBlocks.Integration.Wolverine)
