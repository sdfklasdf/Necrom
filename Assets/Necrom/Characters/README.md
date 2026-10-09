# Character asset reception — WAITING FOR PURCHASE / IMPORT
Reserved project-owned structure for the planned 120-character roster:
- Sprites: selected/imported character textures and sprite sheets.
- Animations: project-owned animation clips and animator controllers.
- Materials: project-owned materials/shader configuration.
- Prefabs: character visuals with stable MON_* bindings.
- Data: reviewed character catalog/profile/visual mappings.

Folders are committed using .gitkeep plus stable Unity folder .meta GUIDs. No real character assets are present here. No Resources.Load path or production combat hookup is added by creating these folders.
Purchased vendor packages may require their own original folder/dependency structure; preserve that structure during initial import, then build project-owned references here. Do not relocate package internals blindly.
Keep existing MON_001...MON_120 IDs stable. Existing ContentDraft/Dummy fixtures remain isolated and are not 120 production assets. Review a representative sample's import, scale, pivot, animations and material compatibility before bulk mapping.
Next state: wait for Minseok's payment and package availability, then perform an approved representative Import AWU. This preparation is not a purchase, Import, license verification or compatibility PASS.
