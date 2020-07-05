namespace MyNamespace.Paxos

open System.Net

open FSharp.Collections
open MyNamespace.util

type Leader(env, id:string, config:Config)=
    inherit Node("leader",id.ep)
    
    let mutable _bn = BallotNumber(0L, id)
    let mutable _active = false
    let mutable _proposals:Map<int64,obj> = Map.empty
    let mutable _config = config

    member me.bn
        with get() = _bn
        and  set(value) = _bn <- value
    member me.active
        with get() = _active
        and  set(value) = _active <- value
    member me.proposals
        with get() = _proposals
        and  set(value) = _proposals <- value
    member me.config
        with get() = _config
        and  set(value) = _config <- value


    override me.body()=
        printf "Here I am Leader: %A\r\n" id
        let s=Scout(env, id, me.config.acceptors, me.bn)
        s.start()
        while true do
            let msg = me.NextMessage()
            if (msg :? ProposeMessage) then
                let msg = msg :?> ProposeMessage
                if not (me.proposals.ContainsKey msg.sn) then
                    me.proposals <-  me.proposals.Add(msg.sn,msg.cmd)
                    if me.active then
                        let c=Commander(env, id, me.config, me.bn, msg.sn, msg.cmd)
                        c.start()
                        ()
                    ()
                ()
            elif (msg :? AdoptedMessage) then
                let msg = msg :?>AdoptedMessage
                if me.bn = msg.bn then
                    let mutable pmax:Map<int64,BallotNumber> = Map.empty
                    for pv in (List.toSeq msg.accepted) do
                        if not (pmax.ContainsKey pv.sn) || pmax.[pv.sn] < pv.bn then
                            pmax <- pmax.Add( pv.sn, pv.bn)
                            me.proposals <- me.proposals.Add( pv.sn, pv.cmd )
                    ()
                Map.iter (fun s c -> 
                            let c=Commander(env, id, me.config, me.bn, s, c)
                            c.start() 
                          ) me.proposals
                me.active <- true
                ()
            elif (msg :? PreemptedMessage) then
                let msg = msg :?>PreemptedMessage 
                if  me.bn < msg.bn then
                    me.bn <- BallotNumber(msg.bn.round+1L, id)
                    let s=Scout(env, id, me.config.acceptors, me.bn)
                    s.start()
                    ()
                me.active <- false
                ()
            else
                printfn "Leader: unknown msg type\r\n"
                ()


// from utils import BallotNumber
// from process import Process
// from cmder import cmder
// from scout import Scout
// from message import ProposeMessage,AdoptedMessage,PreemptedMessage

// class Leader(Process):
//   def __init__(self, env, id, config):
//     Process.__init__(self, env, id)
//     self.bn = BallotNumber(0, self.id)
//     self.active = False
//     self.proposals = {}
//     self.config = config
//     self.env.addProc(self)

//   def body(self):
//     print "Here I am: ", self.id
//     Scout(self.env, "scout:%s:%s" % (str(self.id), str(self.bn)),
//           self.id, self.config.acceptors, self.bn)
//     while True:
//       msg = self.getNextMessage()
//       if isinstance(msg, ProposeMessage):
//         if msg.sn not in self.proposals:
//           self.proposals[msg.sn] = msg.cmd
//           if self.active:
//             cmder(self.env,
//                       "cmder:%s:%s:%s" % (str(self.id),
//                                               str(self.bn),
//                                               str(msg.sn)),
//                       self.id, self.config.acceptors, self.config.replicas,
//                       self.bn, msg.sn, msg.cmd)

//       elif isinstance(msg, AdoptedMessage):
//         if self.bn == msg.bn:
//           pmax = {}
//           for pv in msg.accepted:
//             if pv.sn not in pmax or \
//                   pmax[pv.sn] < pv.bn:
//               pmax[pv.sn] = pv.bn
//               self.proposals[pv.sn] = pv.cmd
//           for sn in self.proposals:
//             cmder(self.env,
//                       "cmder:%s:%s:%s" % (str(self.id),
//                                               str(self.bn),
//                                               str(sn)),
//                       self.id, self.config.acceptors, self.config.replicas,
//                       self.bn, sn, self.proposals.get(sn))
//           self.active = True
//       elif isinstance(msg, PreemptedMessage):
//         if msg.bn > self.bn:
//           self.bn = BallotNumber(msg.bn.round+1,
//                                             self.id)
//           Scout(self.env, "scout:%s:%s" % (str(self.id),
//                                            str(self.bn)),
//                 self.id, self.config.acceptors, self.bn)
//         self.active = False
//       else:
//         print "Leader: unknown msg type"