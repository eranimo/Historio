# Design

## Data Types
Static classes with enums and constants

- Tile
- District
- Improvement

## Components
Including inherited components.
Grouped by area.

Core:
- Location
- Sprite

World:
- TileData
- ResourceNode

Simulation:
- DistrictData
- ImprovementData
- ConstructionSite
  - DistrictConstructionSite
  - ImprovementConstructionSite
- JobProvider

View state:
- TileViewState
- ViewStateNode

Units:
- ActionQueue
- Movement

## Entities
- Tile
  - TileData
  - Location
  - TileViewState
  - ViewStateNode (if owned by Country)
  - CountryTile (relation to Country)
  - SettlementTile (relation to Settlement)
- Unit
  - UnitData
  - Location
  - ActionQueue
  - Movement
  - ViewStateNode
- Settlement
  - SettlementData
  - CapitalSettlement (relation to Country, if capital)
- District
  - DistrictData
  - Sprite
  - Location
  - JobProvider
  - DistrictOwner (relation to Country)
- Improvement
  - ImprovementData
  - Sprite
  - JobProvider
  - Location
  - ImprovementOwner (relation to Country)
- Construction
  - ConstructionSite
  - Location
  - JobProvider