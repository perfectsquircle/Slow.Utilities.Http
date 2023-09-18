pack: clean
	dotnet pack --configuration Release . -o ./out/

push-local: pack
	dotnet nuget push -s Local out/*.nupkg

clean:
	rm -rf ./out

.PHONY: pack, push-local, clean
.DEFAULT: pack