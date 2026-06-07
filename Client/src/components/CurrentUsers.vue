<template>
  <div>
    <h1>WebSocket: Current Users</h1>
    <p v-for="message in messages">{{ message }}</p>
  </div>
</template>

<script>
import { onUnmounted } from 'vue';

export default {
    name: "CurrentUsers",
    data() {
        return {
            //TODO variables
            websocket_url: "/api/ws",
            messages: [],
            socket: null
        }
    },
    methods: {
        send(){
          if(!this.socket || this.socket.readyState !== WebSocket.OPEN )
            return;
          this.socket.send("Hello from client " + new Date().toISOString());
          console.log("Sent message: " + "Hello from client " + new Date().toISOString());
        },
        connect(){
          this.socket = new WebSocket(this.websocket_url);
          
          this.socket.onopen = function(e) {
            console.log("Websocket connection established");
          };
          this.socket.onerror = function(e) {
            console.log("Websocket error: " + e);
          };
          this.socket.onmessage = (e) => {
            console.log("Received message: " + e.data);
            this.messages.push(e.data);
          }
          onUnmounted(() => {
            if(this.socket){
              this.socket.close();
            }
          });
        }
    },
    mounted(){
        this.connect();
        setInterval(() => {
          this.send();
        }, 1000);
    }
}
</script>

<style>
p {
  font-size: 1.2em;
}
</style>