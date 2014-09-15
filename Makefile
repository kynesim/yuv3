# To compile this you will need;
# apt-get install mono-complete
#

OBJDIR := obj
BINDIR := bin

CSFLAGS := /unsafe
SRCS := MainWindow.cs yuv3.cs AppState.cs DisplayYUVControl.cs YUVFile.cs \
	Constants.cs FileInterfacePanel.cs IStatusNotifier.cs \
	ConsoleStatusNotifier.cs Utils.cs Maths.cs
RSRC_OPTS := -resource:rsrc/bg_1.png,MainBackground.png
LOCATED_SRCS := $(SRCS:%=src/%)

all: dirs $(BINDIR)/yuv3.exe $(BINDIR)/yuv3

$(BINDIR)/yuv3: rsrc/yuv3.sh
	install -m 0755 $^ $@


dirs:
	if [ ! -d $(BINDIR) ]; then mkdir $(BINDIR); fi

clean:
	rm -rf $(BINDIR)


$(BINDIR)/yuv3.exe: $(LOCATED_SRCS) rsrc/bg_1.png
	mcs -pkg:dotnet $(LOCATED_SRCS) $(CSFLAGS) $(RSRC_OPTS) -out:$(BINDIR)/yuv3.exe -main:yuv3main -pkg:dotnet

# End file.

