module MyNamespace.util

// namespace MyNamespace.Paxos

open System
open System.Net

open System
open System.Threading
open System.Threading.Tasks

type Microsoft.FSharp.Control.Async with
    static member AwaitTask (t : Task<'T>, timeout : int) =
        async {
            use cts = new CancellationTokenSource()
            use timer = Task.Delay (timeout, cts.Token)
            let! completed = Async.AwaitTask <| Task.WhenAny(t, timer)
            if completed = (t :> Task) then
                cts.Cancel ()
                let! result = Async.AwaitTask t
                return Some result
            else return None
        }

type IPEndPoint with
    member x.txt = x.ToString()

type String with
    member x.ep = 
        let ps=x.Split(":")
        new IPEndPoint(IPAddress.Parse(ps.[0]),int(ps.[1]))


// [<CustomEquality; CustomComparison>]
// type NodeId (ep:IPEndPoint)=

//     static member New(ip, port)=
//                 new NodeId( new IPEndPoint(IPAddress.Parse(ip),port) )  
//     member me.ep=ep
//     member me.byteslong
//         with get()=
//             let mutable sum=0L
//             for b in me.ep.Address.GetAddressBytes() do
//                 sum <- sum*1000L+ int64(b)
//             sum

//     override me.Equals(yobj) =
//        match yobj with
//          | :? NodeId as o -> 
//                                 ( me.ep.Address.Equals(o.ep.Address) && me.ep.Port=o.ep.Port )
//          | _ -> false
//     interface System.IComparable with
//         member me.CompareTo yobj =
//             match yobj with
//                 | :? NodeId as o ->    
//                                 if me.ep.Address.Equals(o.ep.Address) then
//                                     (me.ep.Port-o.ep.Port)
//                                 elif me.byteslong>o.byteslong then
//                                      1
//                                 else
//                                     -1

//                 | _ -> invalidArg "yobj" "cannot compare values of different types"

    // static member (==) (n0:NodeId, n1:NodeId)=
    //     if n0.ep.Address.Equals(n1.ep.Address) && n0.ep.Port=n1.ep.Port then
    //         true
    //     else
    //         false
    // static member (<) (n0:NodeId, n1:NodeId)=
    //     if n0.ep.Address.Equals(n1.ep.Address) then
    //         (n0.ep.Port<n1.ep.Port)
    //     elif n0.byteslong<n1.byteslong then
    //         true
    //     else
    //         false

// [<CustomEquality; CustomComparison>]
type BallotNumber(round:int64,id:string)=
    member me.round = round
    member me.id= id

    override me.Equals(yobj) =
       match yobj with
         | :? BallotNumber as o -> 
                            ( me.id.ep.Equals(o.id.ep) && me.round=o.round )
         | _ -> false
    interface System.IComparable with
        member me.CompareTo yobj =
            match yobj with
                | :? BallotNumber as o ->    
                                if me.id.ep.Equals(o.id.ep) then
                                    int(me.round - o.round)
                                elif me.id > o.id then
                                     1
                                else
                                    -1

                | _ -> invalidArg "yobj" "cannot compare values of different types"
    // static member (<) (b0:BallotNumber, b1:BallotNumber)=
    //     if b0.id=b1.id then
    //         (b0.round<b1.round)
    //     elif (b0.id) < (b1.id) then
    //         true
    //     else
    //         false

    // static member (==) (b0:BallotNumber, b1:BallotNumber)=
    //     if b0.id=b1.id && b0.round=b1.round then
    //         true
    //     else
    //         false


    // return "BN(%d,%s)" % (self.round, str(self.leader_id))

type PValue(bn:BallotNumber,sn:int64,cmd )=
    member me.bn = bn
    member me.sn = sn
    member me.cmd = cmd
    // return "PV(%s,%s,%s)" % (str(self.bn),
    //                          str(self.sn),
    //                          str(self.cmd))

type cmd(client,req_id,op )=
    member me.client = client
    member me.req_id = req_id
    member me.op = op
    // return "cmd(%s,%s,%s)" % (str(self.client),
    //                               str(self.req_id),
    //                               str(self.op))

type Reconfigcmd(client,req_id,config )=
    member me.client = client
    member me.req_id = req_id
    member me.config = config
    // return "Reconfigcmd(%s,%s,%s)" % (str(self.client),
    //                                       str(self.req_id),
    //                                       str(self.config))

type Config( replicas:string list,acceptors:string list,leaders:string list )=
    member me.replicas = replicas
    member me.acceptors = acceptors
    member me.leaders = leaders
    // return "%s;%s;self.leaders))




// WINDOW = 5

// class BallotNumber(namedtuple('BallotNumber',['round','leader_id'])):
//   __slots__ = ()
//   def __str__(self):
//     return "BN(%d,%s)" % (self.round, str(self.leader_id))

// class PValue(namedtuple('PValue',['bn','sn','cmd'])):
//   __slots__ = ()
//   def __str__(self):
//     return "PV(%s,%s,%s)" % (str(self.bn),
//                              str(self.sn),
//                              str(self.cmd))

// class cmd(namedtuple('cmd',['client','req_id','op'])):
//   __slots__ = ()
//   def __str__(self):
//     return "cmd(%s,%s,%s)" % (str(self.client),
//                                   str(self.req_id),
//                                   str(self.op))

// class Reconfigcmd(namedtuple('Reconfigcmd',['client','req_id','config'])):
//   __slots__ = ()
//   def __str__(self):
//     return "Reconfigcmd(%s,%s,%s)" % (str(self.client),
//                                           str(self.req_id),
//                                           str(self.config))

// class Config(namedtuple('Config',['replicas','acceptors','leaders'])):
//   __slots__ = ()
//   def __str__(self):
//     return "%s;%s;self.leaders))
