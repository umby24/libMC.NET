require 'open-uri'
require 'JSON'

def getTableName(myData)
	name = /<span class=\"mw-headline\" .*?> (.*?) <\/span>/.match(myData)
	return name
end

def getTableId(myData)
	id = /<td.*?> (.*?) <\/td>/m.match(myData)
	return id
end

def getTableFields(mydata)
	fields = {}	
	running = 1
	
	while running == 1
		begin
			subsection = /<tr.*?>(.*?)<\/tr>/m.match(mydata)
			mysub = subsection[1]
			
			if mysub.include?("Packet ID") and mysub.include?("Field Name")
				mydata = mydata.gsub(subsection[0], "")
				next
			end
			
			fieldName = /<td.*?> (.*?) <\/td>/.match(mysub)
			mysub = mysub.gsub(fieldName[0], "")
			
			fieldType = /<td.*?> (.*?) <\/td>/.match(mysub)
			mysub = mysub.gsub(fieldType[0], "")
			
			fields[fieldName[1]] = fieldType[1]
			
			mydata = mydata.gsub(subsection[0], "")
		
		rescue Exception => e
			running = 0
		end
	end
	
	return fields
end

def getTables(myData)
	tempTables = []
	running = 1
	
	while running == 1
		begin
			tempTable = /<span class=\"mw-headline\" .*?<table class=\"wikitable\">\n<tr>\n<th> Packet ID <\/th>(.*?)<\/table>/m.match(myData)
			tempTables.push(tempTable[0])
			myData = myData.gsub(tempTable[0], "")
		rescue
			running = 0
			break
		end
	end
	
	return tempTables
end

def stripParenthesis(text)
	return /(.*?)\(.*?\)/m.match(text)[1]
end

def stripSpaces(text)
	text = text.gsub("-", "")
	text = text.gsub("'", "")
	return text.gsub(" ", "")
end

def variableTypeConversion(text)
	text = text.downcase
	text = text.gsub("varint", "int")
	text = text.gsub("unsigned byte", "----")
	text = text.gsub("byte array", "++++")
	text = text.gsub("byte", "sbyte")
	text = text.gsub("----", "byte")
	text = text.gsub("++++", "byte[]")
end

def wsockTypeConversion(text)
	text = text.gsub("Unsigned Byte", "----")
	text = text.gsub("Byte array", "++++")
	text = text.gsub("Byte", "SByte")
	text = text.gsub("----", "Byte")
	text = text.gsub("++++", "Send")
end

def arrayPrinter(myArr, client)
	output = ""
	
	myArr.each do |e|
		if client == true
			output += "    public struct CB#{stripSpaces(e["Name"])} : IPacket {\n"
		else
			output += "    public struct SB#{stripSpaces(e["Name"])} : IPacket {\n"
		end
		
		e["Fields"].each_key do |z|
			output += "        public #{variableTypeConversion(e["Fields"][z])} #{stripSpaces(z)} { get; set; }\n"
		end
		
		output += "\n        public void Read(Wrapped wSock) {\n"
		
		e["Fields"].each_key do |z|
			output += "            #{stripSpaces(z)} = wSock.read#{wsockTypeConversion(e["Fields"][z])}();\n"
		end
		
		output += "        }\n\n"
		
		output += "        public void Write(Wrapped wSock) {\n"
		output += "            wSock.writeVarInt(#{e["Id"]});\n"
		
		e["Fields"].each_key do |z|
			output += "            wSock.write#{wsockTypeConversion(e["Fields"][z])}(#{stripSpaces(z)});\n"
		end
		
		output += "            wSock.Purge();\n"
		output += "        }\n"
		output += "    }\n\n"
	end

	return output
end

def arrayBuilder(myArr, test=false)
	tempArr = []
	
	myArr.each do |z|
		tempArr.push(Hash.new())
		
		tableName = getTableName(z)
		tableId = getTableId(z)
		
		tempArr[tempArr.length - 1]["Name"] = tableName[1]
		
		if test == false
			tempArr[tempArr.length - 1]["Id"] = tableId[1][0, tableId[1].index("</td>")].gsub("\n", "")
		else
			tempArr[tempArr.length - 1]["Id"] = tableId[1]
		end
		
		tempArr[tempArr.length - 1]["Fields"] = getTableFields(z)
	end
	
	return tempArr
end

puts "Beginning parse..."

wSock = open("http://wiki.vg/api.php?format=json&action=parse&page=Protocol&prop=text")
content = wSock.sysread(10000000)
wSock.close()

jsonObj = JSON.parse(content)
data = jsonObj["parse"]["text"]["*"]

puts "Extracting Sections..."

# Retreives the chunk we're looking for (All packet info)

filtered = /title=\"Edit section: Handshaking\">edit<\/a>\]<\/span>(.*?)<span class=\"mw-headline\" id=\"See_Also\"> See Also <\/span>/m.match(data)

data = filtered[1] #Sets it as our first capture.

# Next, we will create each packet-type section.
# Handshaking, Play [Clientbound], Play [Serverbound], Status [Clientbound], Status [Serverbound], and Login [C/S].

handshakeSection = /<span class=\"mw-headline\" id=\"Handshaking\"> Handshaking <\/span>(.*?)<span class=\"mw-headline\" id=\"Play\"> Play <\/span>/m.match(data)
handshakeSectionData = handshakeSection[1]

playSection = /<span class=\"mw-headline\" id=\"Play\"> Play <\/span>(.*?)<span class=\"mw-headline\" id=\"Status\"> Status <\/span>/m.match(data)
playSectionData = playSection[1]

statusSection = /<span class=\"mw-headline\" id=\"Status\"> Status <\/span>(.*?)<span class=\"mw-headline\" id=\"Login\"> Login <\/span>/m.match(data)
statusSectionData = statusSection[1]

loginSection = /<span class=\"mw-headline\" id=\"Login\"> Login <\/span>(.*)/m.match(data)
loginSectionData = loginSection[1]

puts "Sections extracted, getting Clientbound / Serverbound for each section."

playClientSection = /<span class=\"mw-headline\" id=\"Clientbound\">(.*?)<span class=\"mw-headline\" id=\"Serverbound_2\">/m.match(playSectionData)[1]
playServerSection = /<span class=\"mw-headline\" id=\"Serverbound_2\">(.*)/m.match(playSectionData)[1]

statusClientSection = /<span class=\"mw-headline\" id=\"Clientbound_2\">(.*?)<span class=\"mw-headline\" id=\"Serverbound_3\">/m.match(statusSectionData)[1]
statusServerSection = /<span class=\"mw-headline\" id=\"Serverbound_3\">(.*)/m.match(statusSectionData)[1]

loginClientSection = /<span class=\"mw-headline\" id=\"Clientbound_3\">(.*?)<span class=\"mw-headline\" id=\"Serverbound_4\">/m.match(loginSectionData)[1]
loginServerSection = /<span class=\"mw-headline\" id=\"Serverbound_4\">(.*)/m.match(loginSectionData)[1]

puts "Client/Serverbound extracted. Extracting tables."

handServerTables = getTables(handshakeSectionData)

playClientTables = getTables(playClientSection)
playServerTables = getTables(playServerSection)

statusClientTables = getTables(statusClientSection)
statusServerTables = getTables(statusServerSection)

loginClientTables = getTables(loginClientSection)
loginServerTables = getTables(loginServerSection)

puts "Beginning code creation..."

iPacketBase = <<lines
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CWrapped;

namespace libMC.NET.Packets {
    public interface IPacket {
        void Read(Wrapped wSock);
        void Write(Wrapped wSock);
    }
	
lines

iPacketEnd = <<lines
}
lines

generatedFile = iPacketBase # Set down the file base...

puts "Building Arrays..."

handServerArr = arrayBuilder(handServerTables)

playClientArr = arrayBuilder(playClientTables)

playServerArr = arrayBuilder(playServerTables)

statusClientArr = arrayBuilder(statusClientTables)

statusServerArr = arrayBuilder(statusServerTables, true)

loginClientArr = arrayBuilder(loginClientTables)

loginServerArr = arrayBuilder(loginServerTables)

puts "Arrays created."
puts "Building code!\n"

generatedFile += "    // -- Status 0: Handshake\n" + arrayPrinter(handServerArr, false)

generatedFile += "    // -- Status 1: Login\n" + arrayPrinter(loginServerArr, false) + arrayPrinter(loginClientArr, true)

generatedFile += "\n    // -- Status 2: Status\n" + arrayPrinter(statusServerArr, false) + arrayPrinter(statusClientArr, true)

generatedFile += "\n    // -- Status 3: Play\n" + arrayPrinter(playServerArr, false) + arrayPrinter(playClientArr, true)

aFile = File.new("Output/PlayClientTables.html", "w+")
aFile.syswrite(playClientTables)
aFile.close()

aFile = File.new("Output/PlayClientArr.html", "w+")
aFile.syswrite(playClientArr)
aFile.close()

aFile = File.new("Output/IPacket.cs", "w+")
aFile.syswrite(generatedFile)
aFile.close()

puts "Done!"
gets()


