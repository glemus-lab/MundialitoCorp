interface Props {
  message: string | null;
}

export default function SuccessMessage({ message }: Props) {
    if (!message || message.length === 0)
        return null;

    return(
        <div className="p-3 bg-emerald-50 text-emerald-700 rounded-xl text-xs font-bold text-center animate-in zoom-in">
            ✅ {message}
        </div>
    );
}