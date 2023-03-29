options='--nologo --no-restore --blame-hang-timeout 20s'
verbosity='quiet'
testfolder='.'

if [ $# -ne 0 ]
then
    args=( "$@" )

    for ((i = 0; i < $#; i++))
    do
        if [ ${args[i]} = "--no-quiet" ]
        then
            verbosity='minimal'
            continue
        fi

        if [ $((i + 1)) -eq $# ]
        then
            testfolder=${args[i]}
            continue
        fi
    done
fi



PROJECTS=$(find $testfolder -name "*.csproj")

for proj in $PROJECTS
do
    echo "================================================================================ $proj"
    echo ""
    dotnet test $options -v $verbosity $proj -p:CheckEolTargetFramework=false
    echo ""
done