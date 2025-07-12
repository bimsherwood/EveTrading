
$tool = "C:\Users\bimmo\Project\EveTrading\bin\Release\net8.0\publish\EveTrading.exe";

$commodities |
foreach-object {
     &$tool plot;
}

$commodities |
foreach-object {
    &$tool swingtest;
}
