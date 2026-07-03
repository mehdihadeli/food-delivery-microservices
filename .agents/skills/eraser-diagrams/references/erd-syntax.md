# ERD syntax

## Entities

Entities correspond to database tables or similar. Entities contain attributes.

Entity definitions consist of a name followed by `{ }`. For example, `users` is the name of the below entity and it contains attributes `id` and `displayName`.

```
users {
id string
displayName string
}
```

It is possible for entities to contain nothing like the below.

```
users { }
```

Entity names are required to be unique.

## Attributes

Attributes correspond to database table columns or similar.

Attribute definitions occur within an entity definition. They consist of a name, type (optional), and metadata (optional) delimited by the space character. Here is an example:

```
users {
id string pk
}
```

Outside of a definition (e.g. in a relationship statement), attributes are referred to following the entity that they belong to, separated by a `.`. Here is an example:

```
users.teamId > teams.id
```

## Properties

Properties are key-value pairs enclosed in `[ ]` brackets that can be appended to entity definitions. Properties are optional.

It is possible to set multiple properties like shown below:

```
users [icon: user, color: blue] {
 // ...rows
}
```

Here are the properties that are allowed:

| Property | Description | Value | Default value |
|----------|-------------|-------|---------------|
| icon | Attached icons | Icon names (e.g. aws-ec2).| |
| color | Stroke and fill color, when possible | Color name (e.g. blue) or hex code (e.g. #000000) | |
| colorMode | Fill color lightness | pastel, bold, outline | pastel |
| styleMode | Embellishments | shadow, plain, watercolor | shadow |
| typeface | Text typeface | rough, clean, mono | rough |

## Relationships

Relationships show the attribute-level relations between entities.

Here is an example:

```
users.teamId > teams.id
```

It is possible to show omit the attribute-level and simply show entity-level relations like this:

```
users > teams
```

The type of connecting line represents the cardinality between the two entities. Here are the types:

| Connector | Syntax | Description |
|-----------|--------|-------------|
| | `<` | One-to-many |
| | `>` | Many-to-one |
| | `-` | One-to-one |
| | `<>` | Many-to-many |

If a relationship statement contains a name that has not been previously defined as an entity or attribute, an entity or attribute with that name will be created.

Here are the properties that are allowed on relationships (lines):

| Property | Description | Example |
|----------|-------------|---------|
| color | Line color | `users.teamId > teams.id [color: green]` |

## Escape string

Certain reserved characters are not allowed in entity or attribute names. However, you can still use these characters by wrapping the entire entity or attribute name in quotes `" "`.

```
"CI/CD" [icon: gear] {
    id string pk
}
```

## Styling

Styles can be applied at the diagram level. Below is an overview of the options and syntax.

| Property | Values | Default value | Syntax example |
|----------|--------|---------------|----------------|
| colorMode | pastel, bold, outline | pastel | colorMode bold |
| styleMode | shadow, plain, watercolor | shadow | styleMode shadow |
| typeface | rough, clean, mono | rough | typeface clean |
| notation | chen, crows-feet | chen | notation crows-feet |

## Examples

Here are some examples of entity relationship diagrams (ERDs) you can create in Eraser:

### Chat App

```
users [icon: user, color: blue] {
    id string pk
    displayName string
    team_role string
    teams string
}

teams [icon: users, color: blue] {
    id string pk
    name string
}

workspaces [icon: home] {
    id string
    createdAt timestamp
    folderId string
    teamId string
}

folders [icon: folder] {
    id string
    name string
}

chat [icon: message-circle, color: green] {
    duration number
    startedAt timestamp
    endedAt timestamp
    workspaceId string
}

invite [icon: mail, color: green] {
    inviteId string
    type string
    workspaceId string
    inviterId string
}

users.teams <> teams.id
workspaces.folderId > folders.id
workspaces.teamId > teams.id
chat.workspaceId > workspaces.id
invite.workspaceId > workspaces.id
invite.inviterId > users.id
```

### Calendar Booking App

```
User [icon: user] {
  id Int pk
  username String
  email String
  avatar String
  createdDate DateTime
}

Booking [icon: clock] {
  id Int pk
  userId Int
  title String
  startTime DateTime
  endTime DateTime
  location String
  eventTypeId Int
  destinationCalendarId Int
}

EventType [icon: list] {
  id Int pk
  userId Int
  teamId Int
  hidden Boolean
  length Int
}

ApiKey [icon: key]{
  id String pk
  userId Int
  appId String
  hashedKey String
}

App [icon: grid] {
  slug String
  dirName String
  keys Json
  createdAt DateTime
}

Webhook [icon: link] {
  id String pk
  userId Int
  appId String
  active Boolean
}

DestinationCalender [icon: calendar] {
  id Int pk
  userId Int
  integration String
  eventTypeId Int
}

// Booking.eventtType < EventType.id
Webhook.appId > App.slug
Webhook.userId > User.id
// Webhook.eventTypeId > EventType.id
App.slug > ApiKey.appId
User.id < Booking.userId
EventType.userId <> User.id
User.id > ApiKey.userId
DestinationCalender.id > Booking.destinationCalendarId
DestinationCalender.userId < User.id
DestinationCalender.eventTypeId < EventType.id
```

### Form Builder App

```
User [icon: user] {
  id Int
  firstname String
  lastname String
  email String
  emailVerified DateTime
}

Form [icon: check-square] {
  id String
  ownerId Int
  name String
  formType FormType
  createdAt DateTime
}

Pipeline [icon: filter] {
  id String
  name String
  formId String
  events PipelineEvent
  createdAt DateTime
}

SessionEvent [icon: zap]{
  id String
  submissionSessionId String
  type String
  createdAt DateTime
}

SubmissionSession [icon: clock]{
  id String
  formId String
  createdAt DateTime
}

NoCodeForm [icon: check-square] {
  id String
  published Boolean
  closed Boolean
  formId String
}

User.id < Form.ownerId
Form.id < Pipeline.formId
Form.id < SubmissionSession.formId
Form.id < NoCodeForm.formId
SubmissionSession.id < SessionEvent.submissionSessionId
```
