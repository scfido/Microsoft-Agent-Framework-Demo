import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import type { Components } from "react-markdown";

interface MarkdownRendererProps {
  content: string;
}

const components: Components = {
  pre({ children }) {
    return (
      <pre className="overflow-x-auto rounded-lg bg-zinc-900 p-4 text-sm text-zinc-100">
        {children}
      </pre>
    );
  },
  code({ className, children, ...props }) {
    const match = /language-(\w+)/.exec(className || "");
    // If inside a <pre> (code block), just render <code> with language class
    if (match) {
      return (
        <code className={`language-${match[1]}`} {...props}>
          {children}
        </code>
      );
    }
    // Inline code
    return (
      <code
        className="rounded bg-zinc-100 px-1.5 py-0.5 text-sm dark:bg-zinc-800"
        {...props}
      >
        {children}
      </code>
    );
  },
  a({ children, href, ...props }) {
    return (
      <a
        href={href}
        target="_blank"
        rel="noopener noreferrer"
        className="text-primary underline underline-offset-4 hover:opacity-80"
        {...props}
      >
        {children}
      </a>
    );
  },
};

export function MarkdownRenderer({ content }: MarkdownRendererProps) {
  return (
    <div className="prose prose-sm dark:prose-invert max-w-none space-y-2 [&>ul]:list-disc [&>ul]:pl-6 [&>ol]:list-decimal [&>ol]:pl-6">
      <ReactMarkdown remarkPlugins={[remarkGfm]} components={components}>
        {content}
      </ReactMarkdown>
    </div>
  );
}
