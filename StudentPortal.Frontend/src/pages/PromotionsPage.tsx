import { useState } from 'react';
import { Layout } from '../components/Layout';

interface Promo {
  id: number;
  category: string;
  title: string;
  description: string;
  code?: string;
  discount: string;
  validUntil: string;
  emoji: string;
  gradient: string;
  featured?: boolean;
}

const promos: Promo[] = [
  {
    id: 1,
    category: 'Coffee',
    title: 'Coffee House',
    description: 'Show your student ID or use the promo code at any Coffee House location. Valid for any drink.',
    code: 'STUDENT15',
    discount: '15% OFF',
    validUntil: 'Jul 31, 2026',
    emoji: '☕',
    gradient: 'from-amber-400 to-orange-500',
    featured: true,
  },
  {
    id: 2,
    category: 'Transport',
    title: 'Train Tickets',
    description: 'Book any intercity train ticket at a student discount. Apply code at checkout.',
    code: 'TRAIN-STU30',
    discount: '30% OFF',
    validUntil: 'Dec 31, 2026',
    emoji: '🚆',
    gradient: 'from-blue-500 to-indigo-600',
    featured: true,
  },
  {
    id: 3,
    category: 'Food Delivery',
    title: 'FoodRun',
    description: 'Free delivery on your first 3 orders. No minimum order for students.',
    code: 'FREERUN',
    discount: 'FREE DELIVERY',
    validUntil: 'Aug 15, 2026',
    emoji: '🍕',
    gradient: 'from-rose-400 to-pink-600',
  },
  {
    id: 4,
    category: 'Fitness',
    title: 'FitZone Gym',
    description: 'Half-price monthly membership. Full access to equipment and group classes.',
    code: 'FITZONESTU',
    discount: '50% OFF',
    validUntil: 'Sep 1, 2026',
    emoji: '🏋️',
    gradient: 'from-violet-500 to-purple-700',
  },
  {
    id: 5,
    category: 'Books',
    title: 'BookWorld',
    description: 'Discount on all printed books and e-books, in-store and online.',
    code: 'BOOKSTU20',
    discount: '20% OFF',
    validUntil: 'Dec 31, 2026',
    emoji: '📚',
    gradient: 'from-emerald-400 to-teal-600',
  },
  {
    id: 6,
    category: 'Cinema',
    title: 'Student Tuesdays',
    description: 'Every Tuesday, students pay half price for any screening. Bring your student ID.',
    discount: '50% OFF',
    validUntil: 'Ongoing',
    emoji: '🎬',
    gradient: 'from-sky-400 to-cyan-600',
  },
  {
    id: 7,
    category: 'Shopping',
    title: 'TechStore',
    description: 'Student discount on laptops, accessories, and software. Verify with your university email.',
    code: 'TECHSTU10',
    discount: '10% OFF',
    validUntil: 'Oct 31, 2026',
    emoji: '💻',
    gradient: 'from-slate-500 to-gray-700',
  },
  {
    id: 8,
    category: 'Wellbeing',
    title: 'MindSpace App',
    description: 'Free premium access to guided meditation and sleep stories for the full academic year.',
    code: 'MINDSTU',
    discount: 'FREE',
    validUntil: 'Jun 30, 2027',
    emoji: '🧘',
    gradient: 'from-fuchsia-400 to-pink-500',
  },
];

function CopyCode({ code }: { code: string }) {
  const [copied, setCopied] = useState(false);

  function copy() {
    navigator.clipboard.writeText(code).then(() => {
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    });
  }

  return (
    <button
      onClick={copy}
      className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-white/20 hover:bg-white/30 backdrop-blur-sm border border-white/30 transition-all group"
    >
      <span className="font-mono text-xs font-bold text-white tracking-widest">{code}</span>
      <svg className="w-3 h-3 text-white/70 group-hover:text-white transition-colors shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
      </svg>
      {copied && <span className="text-xs text-white font-semibold">Copied!</span>}
    </button>
  );
}

function FeaturedCard({ promo }: { promo: Promo }) {
  return (
    <div className={`relative rounded-2xl bg-gradient-to-br ${promo.gradient} overflow-hidden shadow-lg`}>
      <div className="absolute top-0 right-0 w-40 h-40 opacity-10">
        <div className="text-[9rem] leading-none select-none">{promo.emoji}</div>
      </div>
      <div className="relative p-6">
        <span className="inline-block text-xs font-semibold uppercase tracking-widest text-white/70 mb-3">{promo.category}</span>
        <div className="flex items-end gap-3 mb-2">
          <span className="text-4xl font-black text-white leading-none">{promo.discount}</span>
        </div>
        <h3 className="text-xl font-bold text-white mb-1">{promo.title}</h3>
        <p className="text-sm text-white/80 leading-relaxed mb-4">{promo.description}</p>
        <div className="flex items-center gap-3 flex-wrap">
          {promo.code && <CopyCode code={promo.code} />}
          <span className="text-xs text-white/60">until {promo.validUntil}</span>
        </div>
      </div>
    </div>
  );
}

function RegularCard({ promo }: { promo: Promo }) {
  return (
    <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden hover:shadow-md hover:-translate-y-0.5 transition-all duration-200">
      <div className={`bg-gradient-to-br ${promo.gradient} p-5 flex items-center justify-between`}>
        <span className="text-4xl">{promo.emoji}</span>
        <span className="text-lg font-black text-white">{promo.discount}</span>
      </div>
      <div className="p-4">
        <span className="text-xs font-semibold uppercase tracking-wider text-gray-400">{promo.category}</span>
        <h3 className="font-bold text-gray-900 mt-0.5 mb-1">{promo.title}</h3>
        <p className="text-xs text-gray-500 leading-relaxed mb-3">{promo.description}</p>
        {promo.code ? (
          <div className="flex items-center gap-2">
            <CodeChip code={promo.code} />
          </div>
        ) : null}
        <p className="text-xs text-gray-400 mt-2">Valid until {promo.validUntil}</p>
      </div>
    </div>
  );
}

function CodeChip({ code }: { code: string }) {
  const [copied, setCopied] = useState(false);

  function copy() {
    navigator.clipboard.writeText(code).then(() => {
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    });
  }

  return (
    <button
      onClick={copy}
      className="flex items-center gap-1.5 px-2.5 py-1 rounded-lg border border-dashed border-gray-300 hover:border-indigo-400 hover:bg-indigo-50 transition-all group"
    >
      <span className="font-mono text-xs font-bold text-gray-600 group-hover:text-indigo-700 tracking-wide">{code}</span>
      <svg className="w-3 h-3 text-gray-400 group-hover:text-indigo-500 transition-colors shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
      </svg>
      {copied && <span className="text-xs text-green-600 font-semibold">Copied!</span>}
    </button>
  );
}

export function PromotionsPage() {
  const featured = promos.filter(p => p.featured);
  const regular = promos.filter(p => !p.featured);

  return (
    <Layout>
      <div className="max-w-5xl">
        <div className="mb-8">
          <h1 className="text-2xl font-bold text-gray-900">Student Deals</h1>
          <p className="text-gray-500 mt-1 text-sm">Exclusive discounts and offers just for you.</p>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 mb-8">
          {featured.map(p => <FeaturedCard key={p.id} promo={p} />)}
        </div>

        <h2 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">More offers</h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {regular.map(p => <RegularCard key={p.id} promo={p} />)}
        </div>
      </div>
    </Layout>
  );
}
