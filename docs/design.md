# Design

The game is a ancient society simulator set in a procedurally generated world. Players do not control the population directly, but instead make decisions acting as the "collective leadership" of the country they control.

## World 
The World is split into Tiles, arranged in a hexagon grid. Each tile contains Resource Nodes, which are renewable or finite Resources. Tiles may have Rivers or Roads, which connect to neighboring Tiles.

### Tiles
Tiles have attributes such as Biome (e.g. temperate), Terrain Type (e.g. plains), and Features (e.g. forest)

## Resource Nodes
Resource Nodes represent resources that can be extracted at a given Tile. They might provide a finite amount or they might grow over time.

The tile attributes determine what Resource Nodes exist on a Tile. A Tile may only have one of each Resource Node type.

### Biota
Biota represent plants and animals. Biota compete with each other for space and resources.

Biota may create habitats 

Tile Improvements can change how Biota grow. Animals on a tile with a Pasture will not migrate. Farms grow a specific plant.

Plants grow at a rate dependent on the tile attributes. They may spread to other tiles.

Biota may consume other biota. Biota may require light to grow.

Biota may spread to neighboring tiles if they have reached their 

### Deposits
Deposits representing finite amounts of resources that may be exhausted if used up (e.g. copper ore)

### Visibility
Each Country can only see a certain amount of the World. All their units and their controlled tiles give them an immediate view of the world ("observed"), tiles they have previously visited are termed "unobserved", and all tiles they have never visited are termed "unexplored".

## Countries and Settlements
Countries control Settlements. Settlements have a Territory, which is a set of Tiles they control. Tiles that contain Districts are considered Urban, while Tiles with Improvements are rural. Settlements have a Market, which allows Pops in that Settlement to exchange goods.

### Districts
Districts represent neighborhoods of cities or small villages. They contain many Buildings. Pops work at Buildings.

### Buildings
Districts contain Buildings. Each District has a building capacity, and each building type has a certain capacity, after which no new buildings may be built. The buildings that may be built on a Tile depend on the District on that tile.

Buildings are owned by Pops.

### Improvements
Improvements represent the rural primary sector of the economy. Tiles can contain only one Improvement. Pops work at Improvements.

### Infrastructure
Infrastructure includes Roads, Canals, and Aqueducts. These change the properties of the Tiles they are built on. They are built from Construction Sites.

## Pops
Population is simulated by Pops, which are groups of people of the same Profession that live in a given Tile. This tile is referred to as the Home tile.

### Housing
Buildings provide housing. Pops require housing, otherwise they will be homeless.

### Needs
Pops have Needs, which are Resource required to live, the amount of which is in proportion to their size and dependent on their profession. Pops have an Inventory of goods that they take these needs from. A pop that is not fulfilling its needs will decrease in size and might migrate.

If Pops do not have their required needs, they will Forage from the Tile they are on. This 

## Jobs
Pops may have a Job, which is how they sustain themselves. Pops may split to become employed in a job.

Pops working in Jobs may be paid in wages, in Resources, or not at all. Wages fluctuate based on supply and demand in the given Market. 

Possible job types:
- Construction at Construction Sites
- Production at Buildings
- Extraction at any Tile with Resource Nodes
- Services at Buildings

### Production
Production happens in Buildings by employed Pops. A building might employ multiple types of pops in different number. Production happens in a daily cycle, after which the unit is produced and wages are paid. The output Resource is then is given to the owner Pop.

### Construction
Construction happens in Construction Sites. Construction has a Resource requirement, a labor requirement, and a build time.

Units may create Construction Sites, which can be used to create Districts, Improvements, or Infrastructure. Building construction sites are created immediately.

### Extraction
Pops may extract from Resource Nodes

### Services
Services happen in Buildings. Services perform a continuous task and do not produce anything.

## Units
Units are groups of Pops that can be directly controlled by the Player (or the Country AI). They have a Unit Type, which decides what type of Actions it can perform. Units represent mobile groups of people — everything from slave work gangs, merchant caravans, and legions of soldiers.

Pops who are in Units do not have Jobs. They must get their needs either by Subsistence or the Market.

Units have their own inventory, independent of the Pops that make it up. This is used to store equipment.

Units that are disbanded have their pops return Home.