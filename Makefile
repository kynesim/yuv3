# To compile this you will need;
# apt-get install mono-complete
#

OBJDIR := obj
BINDIR := bin

SRCS := MainWindow.cs yuv3.cs
LOCATED_SRCS := $(SRCS:%=src/%)

all: $(BINDIR)/yuv3.exe


$(BINDIR)/yuv3.exe: $(LOCATED_SRCS)
	mcs $(LOCATED_SRCS) -out:$(BINDIR)/yuv3.exe -main:yuv3main -pkg:dotnet

# End file.

