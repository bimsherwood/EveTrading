
$tool = "C:\Users\bimmo\Project\EveTrading\bin\Release\net8.0\publish\EveTrading.exe";
$commodities = @(

    "Compressed Zeolites",
    "Compressed Bitumens",
    "Compressed Sylvite",
    "Compressed Coesite",
    
    "Compressed Plagioclase",
    "Compressed Pyroxeres",
    "Compressed Veldspar",
    "Compressed Scordite",
    
    "Toxic Metals",
    "Reactive Metals",
    "Precious Metals",
    "Chiral Structures",
    "Silicon",
    
    "Mechanical Parts",
    "Consumer Electronics",
    "Construction Blocks",
    "Robotics",
    "Smartfab Units")

$commodities |
foreach-object {
     &$tool plot $_;
}

$commodities |
foreach-object {
    &$tool swingtest $_;
}
