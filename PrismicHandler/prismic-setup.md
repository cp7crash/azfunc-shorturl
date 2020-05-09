# Overview
This extension allows short URLs to be fetched from prismic.io, using a custom content type.

# Setup
1. Create a new content type in prismic using the definition in `ct-short-url.json`
0. Note the name of this content-type and the repository name
0. Add/Update the `CmsSettings` variable to define your repository `repo`, which `ref` you want to use (e.g. master for live) and the name of your custom `type`
0. Add a web hook for the document publish event, pointing to `https://your.url/yoursecret` adding your secret to `CmsRefreshKey`

See [Prismic API](https://prismic.io/docs/rest-api/basics/introduction-to-the-content-query-api) for more info.