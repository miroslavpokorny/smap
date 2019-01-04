using CommandLine;

namespace smap
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input directory where scanned pages are located or input file to decode based on mode")]
        public string Input { get; set; }

        [Option('m', "mode", Required = true, HelpText = "Specifies mode of program. [Encode|0] to encode data to PDF or [Decode|1] to decode data from image)")]
        public ProgramMode Mode { get; set; }
        
        [Option('o', "output", Default = "output", HelpText = "Path to output directory (decode mode) or output file (encode mode)")]
        public string Output { get; set; }
    }
}