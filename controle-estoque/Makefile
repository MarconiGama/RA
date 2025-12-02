ZIPNAME=controle-estoque.zip

default: zip

zip:
	zip -r $(ZIPNAME) backend frontend docker-compose.yml .devcontainer README.md

clean:
	rm -f $(ZIPNAME)
