# GraphSettings

A struct to hold settings related to the graph's display.

## Public Fields

-   `float xMin`: The minimum x-axis value.
-   `float xMax`: The maximum x-axis value.
-   `float yMin`: The minimum y-axis value.
-   `float yMax`: The maximum y-axis value.
-   `float zMin`: The minimum z-axis value.
-   `float zMax`: The maximum z-axis value.
-   `float step`: The step size for evaluating the graph.

## Public Methods

-   `GraphSettings(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax, float step)`
    -   Constructor for the GraphSettings struct. Initializes all the graph settings.
-   `string ToJSON()`
    -   Converts the GraphSettings struct to a JSON string representation.