interface Props {
  messages: string[] | null;
}

export default function ErrorMessage({ messages }: Props) {
  if (!messages || messages.length === 0) return null;
  return (
    <div className="p-3 bg-red-50 border border-red-100 text-red-700 rounded-xl text-xs font-bold animate-in fade-in slide-in-from-top-1 mb-4">
      <div className="flex flex-col gap-1">
        {messages.map((msg, index) => (
          <div key={index} className="flex items-start gap-2">
            <span>❌</span>
            <span>{msg}</span>
          </div>
        ))}
      </div>
    </div>
  );
}