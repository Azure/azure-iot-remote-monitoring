var IoTApp =
{
    resources: {},
    createModule: function (namespace, module, dependencies) {
        "use strict";

        var nsparts = namespace.split(".");
        var parent = IoTApp;
        // we want to be able to include or exclude the root namespace so we strip
        // it if it's in the namespace
        if (nsparts[0] === "IoTApp") {
            nsparts = nsparts.slice(1);
        }

        function f() {
            return module.apply(this, dependencies);
        }

        f.prototype = module.prototype;

        var innerModule = new f();

        // loop through the parts and create a nested namespace if necessary
        for (var i = 0, namespaceLength = nsparts.length; i < namespaceLength; i++) {
            var partname = nsparts[i];
            // check if the current parent already has the namespace declared
            // if it isn't, then create it
            if (typeof parent[partname] === "undefined") {
                parent[partname] = (i === namespaceLength - 1) ? innerModule : {};
            }
            // get a reference to the deepest element in the hierarchy so far
            parent = parent[partname];
        }

        return parent;
    }
}