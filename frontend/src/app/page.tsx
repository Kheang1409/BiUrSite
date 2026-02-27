import { MainLayout } from "@/components/layouts/MainLayout";
import { Feed } from "@/components/pages/Feed";

export default function Home() {
  return (
    <MainLayout>
      <Feed />
    </MainLayout>
  );
}
