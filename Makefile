# Chimera compiler - Project make file: 
#Tomas Bravo Ortiz A01376668
#Gerardo Ezequiel Magdaleno Hernandez A01377029
#Jesus Heriberto Vasquez Sanchez A01377358

chimera.exe: Driver.cs Scanner.cs Token.cs TokenCategory.cs State.cs Parser.cs \
	SyntaxError.cs
	mcs -out:chimera.exe Driver.cs Scanner.cs Token.cs TokenCategory.cs State.cs \
	Parser.cs SyntaxError.cs Node.cs SpecificNodes.cs
			
clean:
	rm chimera.exe
