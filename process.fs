namespace   MyNamespace.Paxos


open System.Net
open System.Net.Sockets
open System.Diagnostics
open Newtonsoft.Json
open System.Threading
open MyNamespace.util



type Node(name:string, src:IPEndPoint) as me=
    let mutable node = None 
    
    do
        try
            printf "[%A]udpclient %A\r\n" name src
            node <- Some ( new UdpClient( src ) )
            // thread.Start()
        with
            | ex -> printf "exception: %A\r\n" ex 
       
    abstract member body: unit -> unit  
    default me.body()=
        printf "--Here i am: %A\r\n" src
        ()
    member me.run()=
        me.body()
        if node.IsSome then
            node.Value.Close()
        printf "[%A] terminated!!!...\r\n" name 
        () 

    member me.thread=new Thread(me.run)     

    static member New( (ip:string), (port:int))=
        new Node("dummy", new IPEndPoint(IPAddress.Parse(ip),port) )


    member me.Send(ip, port, payload)=
        if node.IsSome then
            let txlen=node.Value.Send(payload, payload.Length, ip, port)
            txlen
        else
            0

    member me.Receive()=
        let remote=ref (new IPEndPoint(IPAddress.Any, 0))
        if node.IsSome then
            let rx=node.Value.Receive(remote)
            printf "Endp %A, payload:%A\r\n" (remote.Value) rx
            ()
        else
            ()

    // member me.SendMessage(endp:IPEndPoint, msg:Message)=
    //     ()
    member me.SendMessage(eptxt:string, msg:Message)=        
        if node.IsSome then
            let dst=eptxt.ep
            let json=JsonConvert.SerializeObject(msg)
            let payload=System.Text.Encoding.UTF8.GetBytes(json)
            // printf "[%A] %A -> %A tx:%A\r\n" name src dst json

            node.Value.Ttl <- 255s;            
            // Thread.Sleep( (new System.Random()).Next(100))  
            let txlen=node.Value.Send(payload, payload.Length, dst)
            txlen
        else
            0

    member me.ParseMessage remote payload =
        let json=System.Text.Encoding.UTF8.GetString(payload)
        let prefix="{\"_kind\":\""
        let mutable msg = new Message("0.0.0.0:0")
        if 0=json.IndexOf(prefix + "Message") then
            msg <- JsonConvert.DeserializeObject<Message>(json)
        elif 0=json.IndexOf(prefix + "P1aMessage") then
            msg <- JsonConvert.DeserializeObject<P1aMessage>(json)
        elif 0=json.IndexOf(prefix + "P1bMessage") then
            msg <- JsonConvert.DeserializeObject<P1bMessage>(json)
        elif 0=json.IndexOf(prefix + "P2aMessage") then
            msg <- JsonConvert.DeserializeObject<P2aMessage>(json)
        elif 0=json.IndexOf(prefix + "P2bMessage") then
            msg <- JsonConvert.DeserializeObject<P2bMessage>(json)
        elif 0=json.IndexOf(prefix + "PreemptedMessage") then
            msg <- JsonConvert.DeserializeObject<PreemptedMessage>(json)
        elif 0=json.IndexOf(prefix + "AdoptedMessage") then
            msg <- JsonConvert.DeserializeObject<AdoptedMessage>(json)
        elif 0=json.IndexOf(prefix + "DecisionMessage") then
            msg <- JsonConvert.DeserializeObject<DecisionMessage>(json)
        elif 0=json.IndexOf(prefix + "RequestMessage") then
            msg <- JsonConvert.DeserializeObject<RequestMessage>(json)
        elif 0=json.IndexOf(prefix + "ProposeMessage") then
            msg <- JsonConvert.DeserializeObject<ProposeMessage>(json)

        assert(json=JsonConvert.SerializeObject(msg))        
        msg.src <- remote.ToString()        
        msg

    member me.NextMessageA()=
        let remote=ref (new IPEndPoint(IPAddress.Any, 0))
        let timer = new Stopwatch()
        timer.Start()
        let rec more()=
            if 20<node.Value.Available then
                printf "[%A] available : %A \r\n" name node.Value.Available
                try 
                    Thread.Sleep(0)
                    let rx=node.Value.Receive(remote)
                    printf "[%A] %A -> %A rx:%A\r\n" name (remote.Value) src (System.Text.Encoding.UTF8.GetString(rx))
                    me.ParseMessage remote.Value rx
                with
                    | ex -> printf "[%A]failed to complete read!!! %A\r\n" name ex
                            new Message("0.0.0.0:0")
            elif 5000L<timer.ElapsedMilliseconds then
                printf "[%A] timeout!!! \r\n" name
                new Message("0.0.0.0:0")
            else
                System.Threading.Thread.Sleep(0)
                more()

        if node.IsNone then
            new Message("0.0.0.0:0") //dummy invalid message
        else
            let msg =  more()
            msg


    member me.NextMessage()=
        let remote=ref (new IPEndPoint(IPAddress.Any, 0))
        let rec more()=
            try
                let t=node.Value.ReceiveAsync()
                let r=Async.AwaitTask(t,50000) |> Async.RunSynchronously

                if r.IsNone then
                    printf "more timed out\r\n"
                    new Message("0.0.0.0:0")
                else
                    me.ParseMessage r.Value.RemoteEndPoint r.Value.Buffer
            with
                | ex -> printf "[%A]failed to complete read!!! %A\r\n" name ex.Message
                        new Message("0.0.0.0:0")

        if node.IsNone then
            printf "node.IsNone\r\n"
            new Message("0.0.0.0:0") //dummy invalid message
        else
            let msg =  more()
            msg



// import multiprocessing
// from threading import Thread

// class Process(Thread):
//   def __init__(self, env, id):
//     super(Process, self).__init__()
//     self.inbox = multiprocessing.Manager().Queue()
//     self.env = env
//     self.id = id

//   def run(self):
//     try:
//       self.body()
//       self.env.removeProc(self.id)
//     except EOFError:
//       print "Exiting.."

//   def getNextMessage(self):
//     return self.inbox.get()

//   def sendMessage(self, dst, msg):
//     self.env.sendMessage(dst, msg)

//   def deliver(self, msg):
//     self.inbox.put(msg)