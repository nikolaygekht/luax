Support for LuaX syntax for editors based on Colorer-take5: Eclipse, FAR Editor, XCE

See more at colorer-take5 website: http://colorer.sourceforge.net/

How to install:

Copy to `hrc/local` directory in colorer-take base directory

Add following code to appropriate location of the `proto.hrc` file.
```xml
<prototype name="luax" group="local" description="LuaX grammar">
    <location link="local/luax.hrc"/>
    <filename>/\.luax$/i</filename>
  </prototype>
```

