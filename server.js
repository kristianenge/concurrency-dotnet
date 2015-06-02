var http = require('http');

var server = http.createServer(function(req, res) {


  res.writeHead(200);
  
  setInterval(function() {
          res.end('Hello World\n');
      },200);

  console.log("Returning Hello HTTP");
});

function sleep(milliseconds) {
	  var start = new Date().getTime();
	  for (var i = 0; i < 1e7; i++) {
	    if ((new Date().getTime() - start) > milliseconds){
	      break;
	    }
	  }
  };
console.log("Server listening to 3000");
server.listen(3000);