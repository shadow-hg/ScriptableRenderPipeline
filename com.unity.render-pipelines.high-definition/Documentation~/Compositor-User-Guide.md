# Compositor: User Guide
To access the HDRP Compositor tool select **Window > Render Pipeline > HD Render Pipeline Compositor** from the menu in the Unity Editor. From this window you can control the compositor configuration. The first time you open the compositor window, the compositor will automatically use a default *"pass-through"* composition profile that simply forwards the output of the main camera to the final composed frame. You can edit this profile or you can load another one from disk.


## Layer Types
The HDRP Compositor tool typically handles two types of layers: 
- **Compositor layers**: are defined in the [Composition Graph](#composition-graph). The composition graph defines the number of layers and how they will be combined, but does not define what are the sources of their content.
- **Sub-layers**: are defined in the compositor window, in the [Render Schedule](#render-schedule)section. By stacking one or more sub-layers, the user can define the source of content for each one of the compositor layers.


## Composition Graph
The final output of the compositor is controlled by a graph of compositing operations which is specified using the *ShaderGraph* tool in Unity. This ShaderGraph should use the Unlit Master Node and the final composed image will be derived from the value of the *Color* pin of this node. All other pins can be left unconnected. Furthermore, the material of the master node should be set to double-sided (for now this is necessary only when the compositor outputs to a render target). 

A composition graph is generally expected to expose two types of input properties:
- Texture2D properties correspond to layer that will be composited to generate the final frame. These properties will automatically appear as compositor layers in the [Render Schedule](#render-schedule) section of the compositor window. The "Mode" option in ShaderGraph corresponds to the default value that the shader will see when the visibility of the layer is toggled off from the Render Schedule list. Please note that by default this value is set to white, but for many compositing operations setting it to black might make more sense.
- Other input properties like colors/floats that controls various aspects of the composition, like tint or brightness. These parameters will appear automatically in the [Composition Parameters](#composition-parameters) section of the compositor window. An example of such parameter is the opacity of the watermarked logo in the following shader graph:

![](Images/Compositor/CompositorSimpleGraph.png)

The compositor settings are saved in a .asset file with the same name as the ShaderGraph. When loading a ShaderGraph, the compositor will also load the settings from the corresponding asset file if one exists, otherwise it will create one with default settings.

## Adding and Removing Composition Layers
New composition layers can be added by creating a new Texture2D input property in the [Composition ShaderGraph](#composition-shadergraph). After saving the ShaderGraph, the new layer will appear automatically in the [Render Schedule](#render-schedule) section of the compositor window. From there, the user can control the [layer properties](#composition-layer-properties) and how these layers will be [filled with content](#sub-layers-adding-content-to-layers). 

Similarly, composition layers can be deleted by removing corresponding Texture 2D properties from the selected composition ShaderGraph. 

## Sub-layers: Adding Content to Layers
Each composition layer can get its content from one or more Sub-Layers. There are three types of Sub-layers:
- **Camera sub-layer:** The source of the content for this layer is a Unity Camera. You can select which camera will be used in the properties of the sub-layer.
- **Video sub-layer:** The source of the content for this layer is a Unity Video Player. You can select which video player will be used in the properties of the sub-layer.
- **Image sub-layer:** The source of the content for this layer is a static image. You can select which image will be used in the properties of the sub-layer.

You can add a sub-layer by first selecting a compositor layer and then press the "Add" drop-down button. From the drop-down you can select the type of sub-layer.

To remove a sub-layer, you have to select it first and then press the "Delete" button. Note that only sub-layers can be deleted. Their parent compositor layers cannot be deleted (the delete button will be inactive when selecting them). To delete these layers, you have to remove the corresponding Texture2D properties from the composition ShaderGraph. 

## Camera Stacking
 When more than one sub-layers are used to specify the content of a (parent) composition layer, then they are "stacked" on top of the same render target. The size and format of this render target is specified in the properties of the parent composition layer and cannot be changed in sub-layers (so all stacked cameras/sub-layers should have the same size and format). The order of the stacking can be changed by re-arranging the sub-layers in the [Render Schedule](#render-schedule) section of the compositor window.
 
The type of stacking operation is controlled in the [Sub-Layer Properties](#sub-layer-properties) section. 

## Render Schedule
The render schedule is a re-orderable list of layers and sub-layers. Sub-layers appear below their corresponding parent layer, and they are also indented, making it easier to see the hierarchical relationship. When multiple sub-layers appear below a parent layer, they form a camera stack. Layers at the top are rendered first. By re-ordering the list, the user can change the rendering order in a camera stack or even move a sub-layer from one parent layer to another (from one stack to another).

## Composition Parameters
If the composition ShaderGraph exposes properties/parameters that are not input layers (for example to control the brightness of the composed image or to add a tint color), then these parameters will appear in this section of the window. It is recommended to use this mechanism to expose such parameters in the compositor window, instead of hard-coding their values in the composition graph. This facilitates sharing [composition profiles](#composition-profile) between projects. 

