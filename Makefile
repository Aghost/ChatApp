getusers:
	@curl -i localhost:5000/api/v1.0/users/

bearer:
	@curl -i localhost:5000/api/v1.0/users/ -H 'Accept: application/json' -H 'Authorization: Bearer $(token)'

getdefault:
	@curl -i localhost:5000/api/v1.0/users/getuser?id=$(id) -H 'Accept: application/json' -H 'Authorization: Bearer $(token)'

json:
	@curl localhost:5000/api/v1.0/users/ | python3 -m json.tool

test:
	@curl -i localhost:5000/api/v1.0/test/

register:
	@curl -i -d '{"FirstName":"Ash", "LastName":"Ketchup", "Email":"Ashman@Kethup.com", "Password":"P0k3B@wl"}' -H 'Content-Type:application/json' -X POST localhost:5000/api/v1.0/accounts/register/

registernew:
	@curl -i -d '{"FirstName":"$(firstname)", "LastName":"$(lastname)", "Email":"$(email)", "Password":"P0k3B@wl"}' -H 'Content-Type:application/json' -X POST localhost:5000/api/v1.0/accounts/register/

login:
	@curl -i -d '{"Email":"Ashman@Kethup.com", "Password":"P0k3B@wl"}' -H 'Content-Type:application/json' -X POST localhost:5000/api/v1.0/accounts/login/

get:
	@curl -i -X GET http://localhost:5000/api/v1.0/users/getuser?id=$(id)

create:
	@curl -i -d '{"FirstName":"klaas", "LastName":"vaak", "Email":"klaas@vaak.nl" }' -H 'Content-Type:application/json' -X POST localhost:5000/api/v1.0/users/
