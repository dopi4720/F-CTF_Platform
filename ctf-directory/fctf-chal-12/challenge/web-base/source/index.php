<?php 
session_start();
if(empty($_SESSION['login']))
{
	$_SESSION['login']=1;
}
else
{
	if(empty($_COOKIE['list']))
	{
		$abc = [];
		if(!empty($_POST))
			array_push($abc,$_POST['a']);

		setcookie('list',serialize($abc));

		foreach ($abc as $key => $value) {
			echo $key,$value;
		}
	}
	else
	{
		$abc = unserialize($_COOKIE['list']);

		if(!empty($_POST) && is_array($abc))
			array_push($abc,$_POST['a']);

		setcookie('list',serialize($abc));

		foreach ($abc as $key => $value) {
			echo $value;
		}
	}
}
Class XYZ{
	public function __toString() { return highlight_file($this->source,true); }
	public function show(){echo $this->source;}
}

?>
<!DOCTYPE html>
<html>
<head>
	<title>Index page</title>
</head>
<body>
<form method="post">
<input type="text" name="a">
<button type="submit" >submit</button>
<div hidden><a href="/target.php"></a></div>
</form>
<!-- <php> Class XYZ{ public function __toString(){return highlight_file($this->source,true);} public function show(){echo $this->source;} }  ?> -->
</body>
</html>