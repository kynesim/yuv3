# To compile this you will need;
# apt-get install mono-complete
#

OBJDIR := obj
BINDIR := bin

SRCS := MainWindow.cs yuv3.cs AppState.cs DisplayYUVControl.cs YUVFile.cs \
	Constants.cs FileInterfacePanel.cs
LOCATED_SRCS := $(SRCS:%=src/%)

all: dirs $(BINDIR)/yuv3.exe

dirs:
	if [ ! -d $(BINDIR) ]; then mkdir $(BINDIR); fi


$(BINDIR)/yuv3.exe: $(LOCATED_SRCS)
	mcs $(LOCATED_SRCS) -out:$(BINDIR)/yuv3.exe -main:yuv3main -pkg:dotnet

# End file.

