import { MainLayout } from "@/components/layouts/MainLayout";
import { PostDetailPage } from "@/components/pages/PostDetailPage";

export default async function PostDetail({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return (
    <MainLayout>
      <PostDetailPage postId={id} />
    </MainLayout>
  );
}
