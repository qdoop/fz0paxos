namespace   MyNamespace.Paxos

open System.Net

open FSharp.Collections
open MyNamespace.util



type Replica(env:Env, id:string, config:Config)=
    inherit Node( "replica", id.ep)

    let mutable _sni=1L
    let mutable _sno=1L
    let mutable _proposals:Map<int64,obj> = Map.empty
    let mutable _decisions:Map<int64,obj> = Map.empty
    let mutable _requests = List.Empty
    let mutable _config = config

    member me.sni         
        with get() = _sni
        and  set(value) = _sni <- value     
    member me.sno         
        with get() = _sno
        and  set(value) = _sno <- value     
    member me.proposals
        with get() = _proposals
        and  set(value) = _proposals <- value   
    member me.decisions 
        with get() = _decisions
        and  set(value) = _decisions <- value   
    member me.requests 
        with get() = _requests 
        and  set(value) = _requests <- value   
    member me.config
        with get() = _config
        and  set(value) = _config <- value   
    
    // do
    //     me.env.addProc(me)

    member me.propose()=
        while me.requests.Length <> 0 && me.sni < me.sno+env.WINDOW do
            // if me.sni > WINDOW && me.sni-WINDOW in me.decisions then
            //     if isinstance(me.decisions[me.sni-WINDOW],Reconfigcmd) then
            //         r,a,l = me.decisions[me.sni-WINDOW].config.split(';')
            //         me.config = Config(r.split(','),a.split(','),l.split(','))
            //         print me.id, ": new config:", me.config
            if  not (me.decisions.ContainsKey me.sni) then
                let cmd = me.requests.Head
                me.proposals <- me.proposals.Add(me.sni, cmd)
                // for leader in me.config.leaders do
                //     me.SendMessage(leader, ProposeMessage(id, me.sni,cmd))
                me.SendMessage(me.config.leaders.Head, ProposeMessage(id, me.sni,cmd))
                ()
            me.sni <- me.sni + 1L

    member me.perform(cmd)=
        let mutable more=true
        let mutable s=1L
        let mutable skip=false 
        while more do
            if cmd=me.decisions.[s] then
                me.sno = me.sno + 1L
                more <-false
                skip <- true
            if me.sno =s then 
                more <- false
            s <- s + 1L
        // if isinstance(cmd, Reconfigcmd) then
        //     me.sno += 1
        //     return
        if not skip then
            printf "%A: perform %A: %A" id me.sno cmd
            me.sno <- me.sno + 1L
        ()

    override me.body()=
        printf "Here I am Replica: %A\r\n" id
        while true do
            let msg = me.NextMessage()
            if (msg :? RequestMessage) then
                let msg = msg :?> RequestMessage
                me.requests <- me.requests @ [msg.cmd]
            elif (msg :? DecisionMessage)then
                let msg = msg :?> DecisionMessage
                me.decisions <- me.decisions.Add(msg.sn, msg.cmd)
            // // while me.sno in me.decisions do
            // //     if me.sno in me.proposals then
            // //         if me.proposals[me.sno]!=me.decisions[me.sno] then
            // //             me.requests.append(me.proposals[me.sno])
            // //         del me.proposals[me.sno]
            // //     me.perform(me.decisions[me.sno])
            else
                printfn "Replica: unknown msg type"
            
            me.propose()
        ()



// from process import Process
// from message import ProposeMessage,DecisionMessage,RequestMessage
// from utils import *
// import time

// class Replica(Process):
//   def __init__(self, env, id, config):
//     Process.__init__(self, env, id)
//     self.sni = self.sno = 1
//     self.proposals = {}
//     self.decisions = {}
//     self.requests = []
//     self.config = config
//     self.env.addProc(self)

//   def propose(self):
//     while len(self.requests) != 0 and self.sni < self.sno+WINDOW:
//       if self.sni > WINDOW and self.sni-WINDOW in self.decisions:
//         if isinstance(self.decisions[self.sni-WINDOW],Reconfigcmd):
//           r,a,l = self.decisions[self.sni-WINDOW].config.split(';')
//           self.config = Config(r.split(','),a.split(','),l.split(','))
//           print self.id, ": new config:", self.config
//       if self.sni not in self.decisions:
//         cmd = self.requests.pop(0)
//         self.proposals[self.sni] = cmd
//         for ldr in self.config.leaders:
//           self.sendMessage(ldr, ProposeMessage(self.id,self.sni,cmd))
//       self.sni +=1

//   def perform(self, cmd):
//     for s in range(1, self.sno):
//       if self.decisions[s] == cmd:
//         self.sno += 1
//         return
//     if isinstance(cmd, Reconfigcmd):
//       self.sno += 1
//       return
//     print self.id, ": perform",self.sno, ":", cmd
//     self.sno += 1

//   def body(self):
//     print "Here I am: ", self.id
//     while True:
//       msg = self.getNextMessage()
//       if isinstance(msg, RequestMessage):
//         self.requests.append(msg.cmd)
//       elif isinstance(msg, DecisionMessage):
//         self.decisions[msg.sn] = msg.cmd
//         while self.sno in self.decisions:
//           if self.sno in self.proposals:
//             if self.proposals[self.sno]!=self.decisions[self.sno]:
//               self.requests.append(self.proposals[self.sno])
//             del self.proposals[self.sno]
//           self.perform(self.decisions[self.sno])
//       else:
//         print "Replica: unknown msg type"
//       self.propose()