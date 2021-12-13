getusers:
	@curl -i localhost:5000/api/v1.0/users/ -H 'Accept: application/json' -H 'Authorization Bearer $(token)'

hack:
	@curl localhost:5000/api/v1.0/test/ | python3 -m json.tool

register:
	echo "register with firstname: $(firstname), lastname: $(lastname), email: $(email)"
	@curl -i -d '{"FirstName":"$(firstname)", "LastName":"$(lastname)", "Email":"$(email)", "Password":"P0k3B@wl"}' -H 'Content-Type:application/json' -X POST localhost:5000/api/v1.0/accounts/register/

login:
	echo "login with email: $(email)"
	@curl -i -d '{"Email":"$(email)", "Password":"P0k3B@wl"}' -H 'Content-Type:application/json' -X POST localhost:5000/api/v1.0/accounts/login/

getrefresh:
	@curl -i -d '{"token":"$(token)", "refreshtoken":"$(refresh)"}' -H 'Content-Type:application/json' -X POST localhost:5000/api/v1.0/accounts/refreshtoken

getuser:
	@curl -i localhost:5000/api/v1.0/users/getuser?id=$(id) -H 'Accept: application/json' -H 'Authorization: Bearer $(token)'
