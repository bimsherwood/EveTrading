
$tool = "C:\Users\bimmo\Project\EveTrading\bin\Release\net8.0\publish\EveTrading.exe";
$commodities = @(
    "Compressed Pyroxeres",
    "Compressed Veldspar",
    "Compressed Scordite",
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
