namespace NDjango.Designer.Parsing

open NDjango.Interfaces

type Parser() = 
    interface IParser with
        member x.Parse template = Seq.empty
