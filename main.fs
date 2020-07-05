
open MyNamespace.Paxos
open MyNamespace.util

open Newtonsoft.Json

[<EntryPoint>]
let main0 argv = 

    //let n0=Node.New("127.0.0.1",0) //11000) use 0 for random port
    //let n1=Node.New("127.0.0.1",11001)

    printf "compare ballots %A\r\n" (BallotNumber( 10L, "0.0.0.0:0") < BallotNumber( 10L, "127.0.0.1:0"))
    printf "compare ballots %A\r\n" (BallotNumber( 0L, "127.0.0.1:0") < BallotNumber( 10L, "127.0.0.1:0"))
    printf "compare ballots %A\r\n" (BallotNumber( -1L, "0.0.0.0:0") < BallotNumber( 0L, "127.0.0.1:10"))

    let msg=P2bMessage("127.0.0.1:0",BallotNumber(10L,"127.0.0.1:1234"),12L)
    let msg=JsonConvert.SerializeObject(msg)
    printf "%s\r\n" msg 
    let o=JsonConvert.DeserializeObject<P2bMessage>(msg)
    printf "%A\r\n" o._kind
    assert(msg=JsonConvert.SerializeObject(o))

    // while true do
    //     printf "looping...\r\n"
    //     n0.Send("127.0.0.1", 11001, System.Text.Encoding.ASCII.GetBytes("Is anybody there?")) |>ignore
    //     System.Threading.Thread.Sleep(1000);
    //     n1.Receive()
    //     System.Threading.Thread.Sleep(1000);

    let config=Config(  ["127.0.0.1:12001";"127.0.0.1:12002";"127.0.0.1:12003"],
                        ["127.0.0.1:11001";"127.0.0.1:11002";"127.0.0.1:11003"],
                        ["127.0.0.1:10001";"127.0.0.1:10002";"127.0.0.1:10003"])

    let env=new Env()

    for x in config.replicas    do 
        let y=new Replica(env,x,config)
        y.thread.Start()
    for x in config.acceptors   do 
        let y=new Acceptor(env,x)
        y.thread.Start()
    for x in config.leaders     do 
        let y=new Leader(env,x, config)
        y.thread.Start()

    let cep="127.0.0.1:14001"
    let client=new Node("client", cep.ep)
    System.Threading.Thread.Sleep(100)
    client.SendMessage(config.replicas.Head, RequestMessage(cep,"cmd0000"))

    0
