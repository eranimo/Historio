# Architecture

## Defs
Static game data

- DistrictType
- ImprovementType
- BuildingType
- ResourceType
- UnitType
- PopProfessionType

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
- BiotaData
- Population

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
- ConstructionSite
  - ConstructionSite
  - Location
  - JobProvider
- Pop
  - Location
  - Population
- Biota
  - Location
  - Population
  - ResourceNode
  - BiotaData
- Deposit
  - Location
  - ResourceNode