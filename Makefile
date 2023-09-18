clean:
	rm -rf ./out

pack: clean
	dotnet pack --configuration Release . -o ./out/

push-local: pack
	dotnet nuget push -s Local out/*.nupkg

