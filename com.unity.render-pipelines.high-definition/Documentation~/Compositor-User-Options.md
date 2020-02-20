# Compositor: User Options
This section provides an outline of the options available on the compositor window.

## Output Options
The output of the compositor (the final composed frame) is only available in game view. If desired, you can get a live preview of the composed frames by undocking the game view from the Unity Editor and have it display on a separate monitor while editing the scene.

The compositor output can be configured using these options:
- **Enable preview:**  When toggled on, the composition output will always be available in the game view, even when editing the scene in edit mode. Toggling this off will increase performance when editing a scene, but then, the compositor output will only be available in *play mode*.
- **Output Camera** The camera that will be used for the compositor's output. It is recommended to use a dedicated camera that is different from the scenes's main camera and point the compositor's camera to a display output that is different from main's camera. If the same display is used for both camera's, the result's will be undefined (the output will depend on which camera has rendered last).
- **Display Output:** Specify on which display the compositor will render the final composed frame. Unity supports up to 8 displays. To see the compositor output, in the upper left corner of the game view select the display that you have specified here.
- **Composition Graph:** Specifies the shader that will be used for composing the final output. By default it is set to a pass-through shader graph that passes a copy of the input to the output. For setting up camera stacking such shader is sufficient, but for more complex compositing operations, the user can define his own graph. For more details see the compositor User Guide.

## Composition Layer Properties
- **Color Buffer Format:** The format that will be used when rendering this layer. 
- **Resolution:** Specifies the pixel resolution of this layer. *Full* corresponds to the resolution of the main camera in the scene. Using half or quarter resolution will improve performance when rendering the corresponding layers, but combining layers of different resolutions might result in artifacts, depending on the content and the compositing operation.
- **Output Renderer:** Specifies that the output of this layer will be re-directed to the surface that is rendered by the selected mesh *renderer*. In particular, the compositor will override and update automatically the "_BaseColorMap" texture of the material that is attached to this renderer. Such configuration is expected to be used when the selected mesh renderer is visible on a camera of another layer.
- **AOVs:** Specifies the type of output variable in this layer. Aside from Color, other output variables that can be selected are Albedo, Normal, Smoothness, etc. This option affects all cameras that re stacked in this layer.

## Sub-Layer Properties
- **Name** The name of the sub-layer. 
- **Source Video** Specifies the video player to be used in a video sub-layer.
- **Source Image** Specifies a static image/texture to be used as background in an image sub-layer.
- **Source Camera** Specifies the camera to be used in a camera sub-layer. By default this is set to the main camera of the scene.
- **Clear Depth** If enabled, the depth buffer will be cleared before drawing the contents of this layer. 
- **Clear Alpha** If enabled, the alpha channel will be cleared before drawing the contents of this layer. If the alpha mask is cleared, then post-processing will affect only the pixels drawn by previous sub-layers. Otherwise it will also affect the pixels drawn from previous layers too.
- **Clear Color** Overrides the clear color mode of the layer. By default the clear color mode of the layer's camera will be used. To override the clear mode of this layer's camera, activate the option by clicking on the checkbox and then select the desired value.
- **Anti-aliasing** Overrides the anti-aliasing mode of the layer. By default the anti-aliasing mode of the layer's camera will be used.
- **Culling Mask** Overrides the culling mask of the layer. By default the culling mask of the layer's camera will be used.
- **Volume Mask** Overrides the culling mask of the layer. By default the volume mask of the layer's camera will be used. This can be used to have unique post-processing effects for each sub-layer.
- **Input Filters** A list of [filters](#sub-layer-filters) that will be applied in this sub-layer, such as chroma keying.

## Sub-Layer Filters
Sub-layer filters are used to apply common color processing operations to layers. The filter list is empty by default and new filters can be added by pressing the "+" button. It is worth noting the the functionality of many filters can be implemented with nodes in the composition graph, but using the built-in filters allows keeping the composition graphs simpler.

These are the filters that are currently available in the Compositor Tool:
- **Chroma keying** Applies a rather basic chroma keying algorithm to the sub-layer. The user options are:
    - **Key color**: the mask color indicates the areas of the image that will be masked (or in other words, the areas that are transparent).
    - **Key Threshold**: Use this parameter to smooth-out the edges of the generated mask. A value of 0 corresponds to sharp edges.
    - **Key Tolerance**: Controls the sensitivity of the mask color parameter. Increasing this value will include more pixels (with a value close to the mask color) in the masked areas. 
    - **Spill Removal**: Use this parameter to change the tint of non-masked areas.
- **Alpha Mask** Takes as input a static texture that overrides the alpha mask of the sub-layer. Post-processing is then applied only on the masked frame regions.


