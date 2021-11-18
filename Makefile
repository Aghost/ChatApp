getusers:
	@curl -i localhost:5000/api/v1.0/users/

test:
	@curl -i localhost:5000/api/v1.0/test/

get:
	@curl -i -X GET http://localhost:5000/api/v1.0/users/getuser?id=$(id)

getfirst:
	@curl -i -X GET http://localhost:5000/api/v1.0/users/getuser?firstName=$(name)

getdefault:
	@curl -i -X GET http://localhost:5000/api/v1.0/users/getuser?id=632e82fe-32f5-43c5-83d3-7e2fec762964

create:
	@curl -i -d '{"FirstName":"klaas", "LastName":"vaak", "Email":"klaas@vaak.nl" }' -H 'Content-Type:application/json' -X POST localhost:5000/api/v1.0/users/
