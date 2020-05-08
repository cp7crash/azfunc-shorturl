# Overview
This extension allows short URLs to be fetched from prismic.io, using a custom content type.

# Setup
0. Create a new content type in prismic using the definition in `ct-short-url.json`
0. Note the name of this content-type and the repository name
0. Add a web hook for the document publish event, pointing to your short url / secret