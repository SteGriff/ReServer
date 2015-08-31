## Writing reconfig.xml

This is an XML file which configures ReServer, the next generation web server.

 * All tags must be closed
 * All tag names must be in lowercase
 * A schema is not required

### Reference config

	<reserver>
		<defaults>
			<content_type>text/html</content_type>
			<index>index.htm</index>
		</defaults>
		<sites>
			<site>
				<name>My Website</name>
				<local>C:/www/inetpub/mysite</local>
				<remote>http://MyWebsite.local/</remote>
				<protect>write</protect>
				<users>
					<user>
						<name>admin</name>
						<password_crypt>boyQD1rQ7j0eMH5zmAMX4w==</password_crypt>
					</user>
				</users>				
			</site>
		</sites>
	</reserver>
	
### Root

The root node is `reserver` and contains `defaults` and `sites` sections.

### Defaults

This section is not implemented in the current version.

### Sites

This is where you configure the website(s) which ReServer will host. There is no enforced limit to how many sites you can have in here.

The `sites` node can only directly contain `site` nodes.

#### Site

Required:

 *	`name`
	The name to use for the website. This is displayed in server logs. It is used in login prompts if authentication is enabled.
 * 	`local`
	The file path where the website files are stored.
 * 	`remote`
	The internet address to bind. For local testing you will need to edit your `hosts` file.
 
Optional features:

 +	`protect`
	  - If this node is not present, no authorisation is enforced;
	  - If this node contains "all" then both reading (GET) and writing (PUT, DELETE) will be protected by authorisation;
	  - If this node contains "write" then only write (PUT, DELETE) will be protected by authorisation;
 +	`users`
	This is the list of users authorised to perform the protected operations described by the `protect` node. It may contain only a list of `user` nodes.
	
#### User

Required:

 *	`name`
	The username which the user will use to login. Recommended to use lowercase only.
 *	`password_crypt`
	The password for the user, a base64-encoded 16 byte PBKDF2 key salted with 'rfc2898-syrup-username' with 'username' replaced with the actual username. The easiest way to obtain this is using the [pkcs5 utility][1].
	
[1]: https://github.com/stegriff/pkcs5
	