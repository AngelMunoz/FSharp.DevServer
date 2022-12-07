function BlogEntry({ blog }: { blog: Blog }) {
  return (
    <section className="perla-blogs__entry">
      <header>{blog.title}</header>
      <time dateTime={blog.date}>Published: {blog.date}</time>
      <p>{blog.summary}</p>
    </section>
  );
}

export function BlogList({ blogs }: { blogs: Blog[] }) {
  const entries = blogs.map((blog) => <BlogEntry blog={blog} />);
  return <article className="perla-blogs">{entries}</article>;
}
