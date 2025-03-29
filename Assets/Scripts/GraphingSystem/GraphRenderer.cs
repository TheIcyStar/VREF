using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// TODO: Do the following when refactoring all scripts
// -move the graph variable enum to its own file
// -change token type to an enum
// -store token type in its own file
// -store the interface in a separate file
// -store LineGraphRenderer in a separate file

// graph variable enum to avoid string comparison
public enum GraphVariable {
    X,
    Y,
    Z,
    Constant
}